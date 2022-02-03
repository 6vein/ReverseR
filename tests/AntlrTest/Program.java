package org.jetbrains.java.decompiler.main;

import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Set;
import org.jetbrains.java.decompiler.main.ClassesProcessor.ClassNode;
import org.jetbrains.java.decompiler.main.collectors.BytecodeMappingTracer;
import org.jetbrains.java.decompiler.main.extern.IFernflowerLogger.Severity;
import org.jetbrains.java.decompiler.main.rels.ClassWrapper;
import org.jetbrains.java.decompiler.main.rels.MethodWrapper;
import org.jetbrains.java.decompiler.modules.decompiler.ExprProcessor;
import org.jetbrains.java.decompiler.modules.decompiler.exps.AnnotationExprent;
import org.jetbrains.java.decompiler.modules.decompiler.exps.ConstExprent;
import org.jetbrains.java.decompiler.modules.decompiler.exps.Exprent;
import org.jetbrains.java.decompiler.modules.decompiler.exps.NewExprent;
import org.jetbrains.java.decompiler.modules.decompiler.exps.TypeAnnotation;
import org.jetbrains.java.decompiler.modules.decompiler.stats.RootStatement;
import org.jetbrains.java.decompiler.modules.decompiler.vars.VarVersionPair;
import org.jetbrains.java.decompiler.modules.renamer.PoolInterceptor;
import org.jetbrains.java.decompiler.struct.StructClass;
import org.jetbrains.java.decompiler.struct.StructField;
import org.jetbrains.java.decompiler.struct.StructMember;
import org.jetbrains.java.decompiler.struct.StructMethod;
import org.jetbrains.java.decompiler.struct.attr.StructAnnDefaultAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructAnnotationAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructAnnotationParameterAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructConstantValueAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructExceptionsAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructGeneralAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructGenericSignatureAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructLineNumberTableAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructMethodParametersAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructTypeAnnotationAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructGeneralAttribute.Key;
import org.jetbrains.java.decompiler.struct.attr.StructMethodParametersAttribute.Entry;
import org.jetbrains.java.decompiler.struct.consts.PrimitiveConstant;
import org.jetbrains.java.decompiler.struct.gen.FieldDescriptor;
import org.jetbrains.java.decompiler.struct.gen.MethodDescriptor;
import org.jetbrains.java.decompiler.struct.gen.VarType;
import org.jetbrains.java.decompiler.struct.gen.generics.GenericClassDescriptor;
import org.jetbrains.java.decompiler.struct.gen.generics.GenericFieldDescriptor;
import org.jetbrains.java.decompiler.struct.gen.generics.GenericMain;
import org.jetbrains.java.decompiler.struct.gen.generics.GenericMethodDescriptor;
import org.jetbrains.java.decompiler.struct.gen.generics.GenericType;
import org.jetbrains.java.decompiler.util.InterpreterUtil;
import org.jetbrains.java.decompiler.util.TextBuffer;

public class ClassWriter {
   private final PoolInterceptor interceptor = DecompilerContext.getPoolInterceptor();
   private static final Key[] ANNOTATION_ATTRIBUTES;
   private static final Key[] PARAMETER_ANNOTATION_ATTRIBUTES;
   private static final Key[] TYPE_ANNOTATION_ATTRIBUTES;
   private static final Map MODIFIERS;
   private static final int CLASS_ALLOWED = 3103;
   private static final int FIELD_ALLOWED = 223;
   private static final int METHOD_ALLOWED = 3391;
   private static final int CLASS_EXCLUDED = 1032;
   private static final int FIELD_EXCLUDED = 25;
   private static final int METHOD_EXCLUDED = 1025;
   private static final int ACCESSIBILITY_FLAGS = 7;

   private static void invokeProcessors(ClassNode node) {
      ClassWrapper wrapper = node.getWrapper();
      StructClass cl = wrapper.getClassStruct();
      InitializerProcessor.extractInitializers(wrapper);
      if (node.type == 0 && !cl.isVersionGE_1_5() && DecompilerContext.getOption("dc4")) {
         ClassReference14Processor.processClassReferences(node);
      }

      if (cl.hasModifier(16384) && DecompilerContext.getOption("den")) {
         EnumProcessor.clearEnum(wrapper);
      }

      if (DecompilerContext.getOption("das")) {
         AssertProcessor.buildAssertions(node);
      }

   }

   public void classLambdaToJava(ClassNode node, TextBuffer buffer, Exprent method_object, int indent, BytecodeMappingTracer origTracer) {
      ClassWrapper wrapper = node.getWrapper();
      if (wrapper != null) {
         boolean lambdaToAnonymous = DecompilerContext.getOption("lac");
         ClassNode outerNode = (ClassNode)DecompilerContext.getProperty("CURRENT_CLASS_NODE");
         DecompilerContext.setProperty("CURRENT_CLASS_NODE", node);
         BytecodeMappingTracer tracer = new BytecodeMappingTracer(origTracer.getCurrentSourceLine());

         try {
            StructClass cl = wrapper.getClassStruct();
            DecompilerContext.getLogger().startWriteClass(node.simpleName);
            if (node.lambdaInformation.is_method_reference) {
               if (!node.lambdaInformation.is_content_method_static && method_object != null) {
                  buffer.append(method_object.toJava(indent, tracer));
               } else {
                  buffer.append(ExprProcessor.getCastTypeName(new VarType(node.lambdaInformation.content_class_name, true)));
               }

               buffer.append("::").append("<init>".equals(node.lambdaInformation.content_method_name) ? "new" : node.lambdaInformation.content_method_name);
            } else {
               StructMethod mt = cl.getMethod(node.lambdaInformation.content_method_key);
               MethodWrapper methodWrapper = wrapper.getMethodWrapper(mt.getName(), mt.getDescriptor());
               MethodDescriptor md_content = MethodDescriptor.parseDescriptor(node.lambdaInformation.content_method_descriptor);
               MethodDescriptor md_lambda = MethodDescriptor.parseDescriptor(node.lambdaInformation.method_descriptor);
               if (!lambdaToAnonymous) {
                  buffer.append('(');
                  boolean firstParameter = true;
                  int index = node.lambdaInformation.is_content_method_static ? 0 : 1;
                  int start_index = md_content.params.length - md_lambda.params.length;

                  for(int i = 0; i < md_content.params.length; ++i) {
                     if (i >= start_index) {
                        if (!firstParameter) {
                           buffer.append(", ");
                        }

                        String parameterName = methodWrapper.varproc.getVarName(new VarVersionPair(index, 0));
                        buffer.append(parameterName == null ? "param" + index : parameterName);
                        firstParameter = false;
                     }

                     index += md_content.params[i].stackSize;
                  }

                  buffer.append(") ->");
               }

               buffer.append(" {").appendLineSeparator();
               tracer.incrementCurrentSourceLine();
               methodLambdaToJava(node, wrapper, mt, buffer, indent + 1, !lambdaToAnonymous, tracer);
               buffer.appendIndent(indent).append("}");
               addTracer(cl, mt, tracer);
            }
         } finally {
            DecompilerContext.setProperty("CURRENT_CLASS_NODE", outerNode);
         }

         DecompilerContext.getLogger().endWriteClass();
      }
   }

   public void classToJava(ClassNode node, TextBuffer buffer, int indent, BytecodeMappingTracer tracer) {
      ClassNode outerNode = (ClassNode)DecompilerContext.getProperty("CURRENT_CLASS_NODE");
      DecompilerContext.setProperty("CURRENT_CLASS_NODE", node);
      int startLine = tracer != null ? tracer.getCurrentSourceLine() : 0;
      BytecodeMappingTracer dummy_tracer = new BytecodeMappingTracer(startLine);

      try {
         invokeProcessors(node);
         ClassWrapper wrapper = node.getWrapper();
         StructClass cl = wrapper.getClassStruct();
         DecompilerContext.getLogger().startWriteClass(cl.qualifiedName);
         int start_class_def = buffer.length();
         this.writeClassDefinition(node, buffer, indent);
         boolean hasContent = false;
         boolean enumFields = false;
         dummy_tracer.incrementCurrentSourceLine(buffer.countLines(start_class_def));
         Iterator var13 = cl.getFields().iterator();

         label299:
         while(true) {
            StructField fd;
            boolean hide;
            boolean isSynthetic;
            do {
               if (!var13.hasNext()) {
                  if (enumFields) {
                     buffer.append(';').appendLineSeparator();
                     dummy_tracer.incrementCurrentSourceLine();
                  }

                  startLine += buffer.countLines(start_class_def);
                  var13 = cl.getMethods().iterator();

                  BytecodeMappingTracer class_tracer;
                  while(var13.hasNext()) {
                     StructMethod mt = (StructMethod)var13.next();
                     hide = mt.isSynthetic() && DecompilerContext.getOption("rsy") || mt.hasModifier(64) && DecompilerContext.getOption("rbr") || wrapper.getHiddenMembers().contains(InterpreterUtil.makeUniqueKey(mt.getName(), mt.getDescriptor()));
                     if (!hide) {
                        int position = buffer.length();
                        int storedLine = startLine;
                        if (hasContent) {
                           buffer.appendLineSeparator();
                           ++startLine;
                        }

                        class_tracer = new BytecodeMappingTracer(startLine);
                        boolean methodSkipped = !this.methodToJava(node, mt, buffer, indent + 1, class_tracer);
                        if (!methodSkipped) {
                           hasContent = true;
                           addTracer(cl, mt, class_tracer);
                           startLine = class_tracer.getCurrentSourceLine();
                        } else {
                           buffer.setLength(position);
                           startLine = storedLine;
                        }
                     }
                  }

                  var13 = node.nested.iterator();

                  while(var13.hasNext()) {
                     ClassNode inner = (ClassNode)var13.next();
                     if (inner.type == 1) {
                        StructClass innerCl = inner.classStruct;
                        isSynthetic = (inner.access & 4096) != 0 || innerCl.isSynthetic();
                        boolean hide = isSynthetic && DecompilerContext.getOption("rsy") || wrapper.getHiddenMembers().contains(innerCl.qualifiedName);
                        if (!hide) {
                           if (hasContent) {
                              buffer.appendLineSeparator();
                              ++startLine;
                           }

                           class_tracer = new BytecodeMappingTracer(startLine);
                           this.classToJava(inner, buffer, indent + 1, class_tracer);
                           startLine = buffer.countLines();
                           hasContent = true;
                        }
                     }
                  }

                  buffer.appendIndent(indent).append('}');
                  if (node.type != 2) {
                     buffer.appendLineSeparator();
                  }
                  break label299;
               }

               fd = (StructField)var13.next();
               hide = fd.isSynthetic() && DecompilerContext.getOption("rsy") || wrapper.getHiddenMembers().contains(InterpreterUtil.makeUniqueKey(fd.getName(), fd.getDescriptor()));
            } while(hide);

            isSynthetic = fd.hasModifier(16384) && DecompilerContext.getOption("den");
            if (isSynthetic) {
               if (enumFields) {
                  buffer.append(',').appendLineSeparator();
                  dummy_tracer.incrementCurrentSourceLine();
               }

               enumFields = true;
            } else if (enumFields) {
               buffer.append(';');
               buffer.appendLineSeparator();
               buffer.appendLineSeparator();
               dummy_tracer.incrementCurrentSourceLine(2);
               enumFields = false;
            }

            this.fieldToJava(wrapper, cl, fd, buffer, indent + 1, dummy_tracer);
            hasContent = true;
         }
      } finally {
         DecompilerContext.setProperty("CURRENT_CLASS_NODE", outerNode);
      }

      DecompilerContext.getLogger().endWriteClass();
   }

   private static void addTracer(StructClass cls, StructMethod method, BytecodeMappingTracer tracer) {
      StructLineNumberTableAttribute table = (StructLineNumberTableAttribute)method.getAttribute(StructGeneralAttribute.ATTRIBUTE_LINE_NUMBER_TABLE);
      tracer.setLineNumberTable(table);
      String key = InterpreterUtil.makeUniqueKey(method.getName(), method.getDescriptor());
      DecompilerContext.getBytecodeSourceMapper().addTracer(cls.qualifiedName, key, tracer);
   }

   private void writeClassDefinition(ClassNode node, TextBuffer buffer, int indent) {
      if (node.type == 2) {
         buffer.append(" {").appendLineSeparator();
      } else {
         ClassWrapper wrapper = node.getWrapper();
         StructClass cl = wrapper.getClassStruct();
         int flags = node.type == 0 ? cl.getAccessFlags() : node.access;
         boolean isDeprecated = cl.hasAttribute(StructGeneralAttribute.ATTRIBUTE_DEPRECATED);
         boolean isSynthetic = (flags & 4096) != 0 || cl.hasAttribute(StructGeneralAttribute.ATTRIBUTE_SYNTHETIC);
         boolean isEnum = DecompilerContext.getOption("den") && (flags & 16384) != 0;
         boolean isInterface = (flags & 512) != 0;
         boolean isAnnotation = (flags & 8192) != 0;
         if (isDeprecated) {
            appendDeprecation(buffer, indent);
         }

         if (this.interceptor != null) {
            String oldName = this.interceptor.getOldName(cl.qualifiedName);
            appendRenameComment(buffer, oldName, ClassWriter.MType.CLASS, indent);
         }

         if (isSynthetic) {
            appendComment(buffer, "synthetic class", indent);
         }

         appendAnnotations(buffer, indent, cl, -1);
         buffer.appendIndent(indent);
         if (isEnum) {
            flags &= -1025;
            flags &= -17;
         }

         appendModifiers(buffer, flags, 3103, isInterface, 1032);
         if (isEnum) {
            buffer.append("enum ");
         } else if (isInterface) {
            if (isAnnotation) {
               buffer.append('@');
            }

            buffer.append("interface ");
         } else {
            buffer.append("class ");
         }

         buffer.append(node.simpleName);
         GenericClassDescriptor descriptor = getGenericClassDescriptor(cl);
         if (descriptor != null && !descriptor.fparameters.isEmpty()) {
            appendTypeParameters(buffer, descriptor.fparameters, descriptor.fbounds);
         }

         buffer.append(' ');
         if (!isEnum && !isInterface && cl.superClass != null) {
            VarType supertype = new VarType(cl.superClass.getString(), true);
            if (!VarType.VARTYPE_OBJECT.equals(supertype)) {
               buffer.append("extends ");
               if (descriptor != null) {
                  buffer.append(GenericMain.getGenericCastTypeName(descriptor.superclass));
               } else {
                  buffer.append(ExprProcessor.getCastTypeName(supertype));
               }

               buffer.append(' ');
            }
         }

         if (!isAnnotation) {
            int[] interfaces = cl.getInterfaces();
            if (interfaces.length > 0) {
               buffer.append(isInterface ? "extends " : "implements ");

               for(int i = 0; i < interfaces.length; ++i) {
                  if (i > 0) {
                     buffer.append(", ");
                  }

                  if (descriptor != null) {
                     buffer.append(GenericMain.getGenericCastTypeName((GenericType)descriptor.superinterfaces.get(i)));
                  } else {
                     buffer.append(ExprProcessor.getCastTypeName(new VarType(cl.getInterface(i), true)));
                  }
               }

               buffer.append(' ');
            }
         }

         buffer.append('{').appendLineSeparator();
      }
   }

   private void fieldToJava(ClassWrapper wrapper, StructClass cl, StructField fd, TextBuffer buffer, int indent, BytecodeMappingTracer tracer) {
      int start = buffer.length();
      boolean isInterface = cl.hasModifier(512);
      boolean isDeprecated = fd.hasAttribute(StructGeneralAttribute.ATTRIBUTE_DEPRECATED);
      boolean isEnum = fd.hasModifier(16384) && DecompilerContext.getOption("den");
      if (isDeprecated) {
         appendDeprecation(buffer, indent);
      }

      if (this.interceptor != null) {
         String oldName = this.interceptor.getOldName(cl.qualifiedName + " " + fd.getName() + " " + fd.getDescriptor());
         appendRenameComment(buffer, oldName, ClassWriter.MType.FIELD, indent);
      }

      if (fd.isSynthetic()) {
         appendComment(buffer, "synthetic field", indent);
      }

      appendAnnotations(buffer, indent, fd, 19);
      buffer.appendIndent(indent);
      if (!isEnum) {
         appendModifiers(buffer, fd.getAccessFlags(), 223, isInterface, 25);
      }

      VarType fieldType = new VarType(fd.getDescriptor(), false);
      GenericFieldDescriptor descriptor = null;
      if (DecompilerContext.getOption("dgs")) {
         StructGenericSignatureAttribute attr = (StructGenericSignatureAttribute)fd.getAttribute(StructGeneralAttribute.ATTRIBUTE_SIGNATURE);
         if (attr != null) {
            descriptor = GenericMain.parseFieldSignature(attr.getSignature());
         }
      }

      if (!isEnum) {
         if (descriptor != null) {
            buffer.append(GenericMain.getGenericCastTypeName(descriptor.type));
         } else {
            buffer.append(ExprProcessor.getCastTypeName(fieldType));
         }

         buffer.append(' ');
      }

      buffer.append(fd.getName());
      tracer.incrementCurrentSourceLine(buffer.countLines(start));
      Exprent initializer;
      if (fd.hasModifier(8)) {
         initializer = (Exprent)wrapper.getStaticFieldInitializers().getWithKey(InterpreterUtil.makeUniqueKey(fd.getName(), fd.getDescriptor()));
      } else {
         initializer = (Exprent)wrapper.getDynamicFieldInitializers().getWithKey(InterpreterUtil.makeUniqueKey(fd.getName(), fd.getDescriptor()));
      }

      if (initializer != null) {
         if (isEnum && initializer.type == 10) {
            NewExprent expr = (NewExprent)initializer;
            expr.setEnumConst(true);
            buffer.append(expr.toJava(indent, tracer));
         } else {
            buffer.append(" = ");
            if (initializer.type == 3) {
               ((ConstExprent)initializer).adjustConstType(fieldType);
            }

            buffer.append(initializer.toJava(indent, tracer));
         }
      } else if (fd.hasModifier(16) && fd.hasModifier(8)) {
         StructConstantValueAttribute attr = (StructConstantValueAttribute)fd.getAttribute(StructGeneralAttribute.ATTRIBUTE_CONSTANT_VALUE);
         if (attr != null) {
            PrimitiveConstant constant = cl.getPool().getPrimitiveConstant(attr.getIndex());
            buffer.append(" = ");
            buffer.append((new ConstExprent(fieldType, constant.value, (Set)null)).toJava(indent, tracer));
         }
      }

      if (!isEnum) {
         buffer.append(";").appendLineSeparator();
         tracer.incrementCurrentSourceLine();
      }

   }

   private static void methodLambdaToJava(ClassNode lambdaNode, ClassWrapper classWrapper, StructMethod mt, TextBuffer buffer, int indent, boolean codeOnly, BytecodeMappingTracer tracer) {
      MethodWrapper methodWrapper = classWrapper.getMethodWrapper(mt.getName(), mt.getDescriptor());
      MethodWrapper outerWrapper = (MethodWrapper)DecompilerContext.getProperty("CURRENT_METHOD_WRAPPER");
      DecompilerContext.setProperty("CURRENT_METHOD_WRAPPER", methodWrapper);

      try {
         String method_name = lambdaNode.lambdaInformation.method_name;
         MethodDescriptor md_content = MethodDescriptor.parseDescriptor(lambdaNode.lambdaInformation.content_method_descriptor);
         MethodDescriptor md_lambda = MethodDescriptor.parseDescriptor(lambdaNode.lambdaInformation.method_descriptor);
         if (!codeOnly) {
            buffer.appendIndent(indent);
            buffer.append("public ");
            buffer.append(method_name);
            buffer.append("(");
            boolean firstParameter = true;
            int index = lambdaNode.lambdaInformation.is_content_method_static ? 0 : 1;
            int start_index = md_content.params.length - md_lambda.params.length;

            for(int i = 0; i < md_content.params.length; ++i) {
               if (i >= start_index) {
                  if (!firstParameter) {
                     buffer.append(", ");
                  }

                  String typeName = ExprProcessor.getCastTypeName(md_content.params[i].copy());
                  if ("<undefinedtype>".equals(typeName) && DecompilerContext.getOption("uto")) {
                     typeName = ExprProcessor.getCastTypeName(VarType.VARTYPE_OBJECT);
                  }

                  buffer.append(typeName);
                  buffer.append(" ");
                  String parameterName = methodWrapper.varproc.getVarName(new VarVersionPair(index, 0));
                  buffer.append(parameterName == null ? "param" + index : parameterName);
                  firstParameter = false;
               }

               index += md_content.params[i].stackSize;
            }

            buffer.append(") {").appendLineSeparator();
            ++indent;
         }

         RootStatement root = classWrapper.getMethodWrapper(mt.getName(), mt.getDescriptor()).root;
         if (!methodWrapper.decompiledWithErrors && root != null) {
            try {
               buffer.append(root.toJava(indent, tracer));
            } catch (Throwable var21) {
               String message = "Method " + mt.getName() + " " + mt.getDescriptor() + " couldn't be written.";
               DecompilerContext.getLogger().writeMessage(message, Severity.WARN, var21);
               methodWrapper.decompiledWithErrors = true;
            }
         }

         if (methodWrapper.decompiledWithErrors) {
            buffer.appendIndent(indent);
            buffer.append("// $FF: Couldn't be decompiled");
            buffer.appendLineSeparator();
         }

         if (root != null) {
            tracer.addMapping(root.getDummyExit().bytecode);
         }

         if (!codeOnly) {
            --indent;
            buffer.appendIndent(indent).append('}').appendLineSeparator();
         }
      } finally {
         DecompilerContext.setProperty("CURRENT_METHOD_WRAPPER", outerWrapper);
      }

   }

   private static String toValidJavaIdentifier(String name) {
      if (name != null && !name.isEmpty()) {
         boolean changed = false;
         StringBuilder res = new StringBuilder(name.length());

         for(int i = 0; i < name.length(); ++i) {
            char c = name.charAt(i);
            if ((i != 0 || Character.isJavaIdentifierStart(c)) && (i <= 0 || Character.isJavaIdentifierPart(c))) {
               res.append(c);
            } else {
               changed = true;
               res.append("_");
            }
         }

         if (!changed) {
            return name;
         } else {
            return res.append("/* $FF was: ").append(name).append("*/").toString();
         }
      } else {
         return name;
      }
   }

   private boolean methodToJava(ClassNode node, StructMethod mt, TextBuffer buffer, int indent, BytecodeMappingTracer tracer) {
      ClassWrapper wrapper = node.getWrapper();
      StructClass cl = wrapper.getClassStruct();
      MethodWrapper methodWrapper = wrapper.getMethodWrapper(mt.getName(), mt.getDescriptor());
      boolean hideMethod = false;
      int start_index_method = buffer.length();
      MethodWrapper outerWrapper = (MethodWrapper)DecompilerContext.getProperty("CURRENT_METHOD_WRAPPER");
      DecompilerContext.setProperty("CURRENT_METHOD_WRAPPER", methodWrapper);

      try {
         boolean isInterface = cl.hasModifier(512);
         boolean isAnnotation = cl.hasModifier(8192);
         boolean isEnum = cl.hasModifier(16384) && DecompilerContext.getOption("den");
         boolean isDeprecated = mt.hasAttribute(StructGeneralAttribute.ATTRIBUTE_DEPRECATED);
         boolean clinit = false;
         boolean init = false;
         boolean dinit = false;
         MethodDescriptor md = MethodDescriptor.parseDescriptor(mt.getDescriptor());
         int flags = mt.getAccessFlags();
         if ((flags & 256) != 0) {
            flags &= -2049;
         }

         if ("<clinit>".equals(mt.getName())) {
            flags &= 8;
         }

         if (isDeprecated) {
            appendDeprecation(buffer, indent);
         }

         if (this.interceptor != null) {
            String oldName = this.interceptor.getOldName(cl.qualifiedName + " " + mt.getName() + " " + mt.getDescriptor());
            appendRenameComment(buffer, oldName, ClassWriter.MType.METHOD, indent);
         }

         boolean isSynthetic = (flags & 4096) != 0 || mt.hasAttribute(StructGeneralAttribute.ATTRIBUTE_SYNTHETIC);
         boolean isBridge = (flags & 64) != 0;
         if (isSynthetic) {
            appendComment(buffer, "synthetic method", indent);
         }

         if (isBridge) {
            appendComment(buffer, "bridge method", indent);
         }

         appendAnnotations(buffer, indent, mt, 20);
         buffer.appendIndent(indent);
         appendModifiers(buffer, flags, 3391, isInterface, 1025);
         if (isInterface && !mt.hasModifier(8) && mt.containsCode()) {
            buffer.append("default ");
         }

         String name = mt.getName();
         if ("<init>".equals(name)) {
            if (node.type == 2) {
               name = "";
               dinit = true;
            } else {
               name = node.simpleName;
               init = true;
            }
         } else if ("<clinit>".equals(name)) {
            name = "";
            clinit = true;
         }

         GenericMethodDescriptor descriptor = null;
         List mask;
         String message;
         if (DecompilerContext.getOption("dgs")) {
            StructGenericSignatureAttribute attr = (StructGenericSignatureAttribute)mt.getAttribute(StructGeneralAttribute.ATTRIBUTE_SIGNATURE);
            if (attr != null) {
               descriptor = GenericMain.parseMethodSignature(attr.getSignature());
               if (descriptor != null) {
                  long actualParams = (long)md.params.length;
                  mask = methodWrapper.synthParameters;
                  if (mask != null) {
                     actualParams = mask.stream().filter(Objects::isNull).count();
                  } else if (isEnum && init) {
                     actualParams -= 2L;
                  }

                  if (actualParams != (long)descriptor.parameterTypes.size()) {
                     message = "Inconsistent generic signature in method " + mt.getName() + " " + mt.getDescriptor() + " in " + cl.qualifiedName;
                     DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
                     descriptor = null;
                  }
               }
            }
         }

         boolean throwsExceptions = false;
         int paramCount = 0;
         if (!clinit && !dinit) {
            boolean thisVar = !mt.hasModifier(8);
            if (descriptor != null && !descriptor.typeParameters.isEmpty()) {
               appendTypeParameters(buffer, descriptor.typeParameters, descriptor.typeParameterBounds);
               buffer.append(' ');
            }

            if (!init) {
               if (descriptor != null) {
                  buffer.append(GenericMain.getGenericCastTypeName(descriptor.returnType));
               } else {
                  buffer.append(ExprProcessor.getCastTypeName(md.ret));
               }

               buffer.append(' ');
            }

            buffer.append(toValidJavaIdentifier(name));
            buffer.append('(');
            mask = methodWrapper.synthParameters;
            int lastVisibleParameterIndex = -1;
            int i = 0;

            label702:
            while(true) {
               if (i >= md.params.length) {
                  List methodParameters = null;
                  if (DecompilerContext.getOption("ump")) {
                     StructMethodParametersAttribute attr = (StructMethodParametersAttribute)mt.getAttribute(StructGeneralAttribute.ATTRIBUTE_METHOD_PARAMETERS);
                     if (attr != null) {
                        methodParameters = attr.getEntries();
                     }
                  }

                  int index = isEnum && init ? 3 : (thisVar ? 1 : 0);
                  int start = isEnum && init ? 2 : 0;

                  for(int i = start; i < md.params.length; ++i) {
                     if (mask == null || mask.get(i) == null) {
                        if (paramCount > 0) {
                           buffer.append(", ");
                        }

                        appendParameterAnnotations(buffer, mt, paramCount);
                        if (methodParameters != null && i < methodParameters.size()) {
                           appendModifiers(buffer, ((Entry)methodParameters.get(i)).myAccessFlags, 16, isInterface, 0);
                        } else if (methodWrapper.varproc.getVarFinal(new VarVersionPair(index, 0)) == 2) {
                           buffer.append("final ");
                        }

                        boolean isVarArg = i == lastVisibleParameterIndex && mt.hasModifier(128);
                        String typeName;
                        if (descriptor != null) {
                           GenericType parameterType = (GenericType)descriptor.parameterTypes.get(paramCount);
                           isVarArg &= parameterType.arrayDim > 0;
                           if (isVarArg) {
                              parameterType = parameterType.decreaseArrayDim();
                           }

                           typeName = GenericMain.getGenericCastTypeName(parameterType);
                        } else {
                           VarType parameterType = md.params[i];
                           isVarArg &= parameterType.arrayDim > 0;
                           if (isVarArg) {
                              parameterType = parameterType.decreaseArrayDim();
                           }

                           typeName = ExprProcessor.getCastTypeName(parameterType);
                        }

                        if ("<undefinedtype>".equals(typeName) && DecompilerContext.getOption("uto")) {
                           typeName = ExprProcessor.getCastTypeName(VarType.VARTYPE_OBJECT);
                        }

                        buffer.append(typeName);
                        if (isVarArg) {
                           buffer.append("...");
                        }

                        buffer.append(' ');
                        String parameterName;
                        if (methodParameters != null && i < methodParameters.size()) {
                           parameterName = ((Entry)methodParameters.get(i)).myName;
                        } else {
                           parameterName = methodWrapper.varproc.getVarName(new VarVersionPair(index, 0));
                        }

                        buffer.append(parameterName == null ? "param" + index : parameterName);
                        ++paramCount;
                     }

                     index += md.params[i].stackSize;
                  }

                  buffer.append(')');
                  StructExceptionsAttribute attr = (StructExceptionsAttribute)mt.getAttribute(StructGeneralAttribute.ATTRIBUTE_EXCEPTIONS);
                  if ((descriptor == null || descriptor.exceptionTypes.isEmpty()) && attr == null) {
                     break;
                  }

                  throwsExceptions = true;
                  buffer.append(" throws ");
                  int i = 0;

                  while(true) {
                     if (i >= attr.getThrowsExceptions().size()) {
                        break label702;
                     }

                     if (i > 0) {
                        buffer.append(", ");
                     }

                     if (descriptor != null && !descriptor.exceptionTypes.isEmpty()) {
                        GenericType type = (GenericType)descriptor.exceptionTypes.get(i);
                        buffer.append(GenericMain.getGenericCastTypeName(type));
                     } else {
                        VarType type = new VarType(attr.getExcClassname(i, cl.getPool()), true);
                        buffer.append(ExprProcessor.getCastTypeName(type));
                     }

                     ++i;
                  }
               }

               if (mask == null || mask.get(i) == null) {
                  lastVisibleParameterIndex = i;
               }

               ++i;
            }
         }

         tracer.incrementCurrentSourceLine(buffer.countLines(start_index_method));
         if ((flags & 1280) != 0) {
            if (isAnnotation) {
               StructAnnDefaultAttribute attr = (StructAnnDefaultAttribute)mt.getAttribute(StructGeneralAttribute.ATTRIBUTE_ANNOTATION_DEFAULT);
               if (attr != null) {
                  buffer.append(" default ");
                  buffer.append(attr.getDefaultValue().toJava(0, BytecodeMappingTracer.DUMMY));
               }
            }

            buffer.append(';');
            buffer.appendLineSeparator();
         } else {
            if (!clinit && !dinit) {
               buffer.append(' ');
            }

            buffer.append('{').appendLineSeparator();
            tracer.incrementCurrentSourceLine();
            RootStatement root = wrapper.getMethodWrapper(mt.getName(), mt.getDescriptor()).root;
            if (root != null && !methodWrapper.decompiledWithErrors) {
               try {
                  BytecodeMappingTracer codeTracer = new BytecodeMappingTracer(tracer.getCurrentSourceLine());
                  TextBuffer code = root.toJava(indent + 1, codeTracer);
                  hideMethod = code.length() == 0 && (clinit || dinit || hideConstructor(node, init, throwsExceptions, paramCount, flags));
                  buffer.append(code);
                  tracer.setCurrentSourceLine(codeTracer.getCurrentSourceLine());
                  tracer.addTracer(codeTracer);
               } catch (Throwable var40) {
                  message = "Method " + mt.getName() + " " + mt.getDescriptor() + " couldn't be written.";
                  DecompilerContext.getLogger().writeMessage(message, Severity.WARN, var40);
                  methodWrapper.decompiledWithErrors = true;
               }
            }

            if (methodWrapper.decompiledWithErrors) {
               buffer.appendIndent(indent + 1);
               buffer.append("// $FF: Couldn't be decompiled");
               buffer.appendLineSeparator();
               tracer.incrementCurrentSourceLine();
            } else if (root != null) {
               tracer.addMapping(root.getDummyExit().bytecode);
            }

            buffer.appendIndent(indent).append('}').appendLineSeparator();
         }

         tracer.incrementCurrentSourceLine();
      } finally {
         DecompilerContext.setProperty("CURRENT_METHOD_WRAPPER", outerWrapper);
      }

      return !hideMethod;
   }

   private static boolean hideConstructor(ClassNode node, boolean init, boolean throwsExceptions, int paramCount, int methodAccessFlags) {
      if (init && !throwsExceptions && paramCount <= 0 && DecompilerContext.getOption("hdc")) {
         ClassWrapper wrapper = node.getWrapper();
         StructClass cl = wrapper.getClassStruct();
         int classAccesFlags = node.type == 0 ? cl.getAccessFlags() : node.access;
         boolean isEnum = cl.hasModifier(16384) && DecompilerContext.getOption("den");
         if (!isEnum && (classAccesFlags & 7) != (methodAccessFlags & 7)) {
            return false;
         } else {
            int count = 0;
            Iterator var10 = cl.getMethods().iterator();

            while(var10.hasNext()) {
               StructMethod mt = (StructMethod)var10.next();
               if ("<init>".equals(mt.getName())) {
                  ++count;
                  if (count > 1) {
                     return false;
                  }
               }
            }

            return true;
         }
      } else {
         return false;
      }
   }

   private static void appendDeprecation(TextBuffer buffer, int indent) {
      buffer.appendIndent(indent).append("/** @deprecated */").appendLineSeparator();
   }

   private static void appendRenameComment(TextBuffer buffer, String oldName, ClassWriter.MType type, int indent) {
      if (oldName != null) {
         buffer.appendIndent(indent);
         buffer.append("// $FF: renamed from: ");
         switch(type) {
         case CLASS:
            buffer.append(ExprProcessor.buildJavaClassName(oldName));
            break;
         case FIELD:
            String[] fParts = oldName.split(" ");
            FieldDescriptor fd = FieldDescriptor.parseDescriptor(fParts[2]);
            buffer.append(fParts[1]);
            buffer.append(' ');
            buffer.append(getTypePrintOut(fd.type));
            break;
         default:
            String[] mParts = oldName.split(" ");
            MethodDescriptor md = MethodDescriptor.parseDescriptor(mParts[2]);
            buffer.append(mParts[1]);
            buffer.append(" (");
            boolean first = true;
            VarType[] var9 = md.params;
            int var10 = var9.length;

            for(int var11 = 0; var11 < var10; ++var11) {
               VarType paramType = var9[var11];
               if (!first) {
                  buffer.append(", ");
               }

               first = false;
               buffer.append(getTypePrintOut(paramType));
            }

            buffer.append(") ");
            buffer.append(getTypePrintOut(md.ret));
         }

         buffer.appendLineSeparator();
      }
   }

   private static String getTypePrintOut(VarType type) {
      String typeText = ExprProcessor.getCastTypeName(type, false);
      if ("<undefinedtype>".equals(typeText) && DecompilerContext.getOption("uto")) {
         typeText = ExprProcessor.getCastTypeName(VarType.VARTYPE_OBJECT, false);
      }

      return typeText;
   }

   private static void appendComment(TextBuffer buffer, String comment, int indent) {
      buffer.appendIndent(indent).append("// $FF: ").append(comment).appendLineSeparator();
   }

   private static void appendAnnotations(TextBuffer buffer, int indent, StructMember mb, int targetType) {
      Set filter = new HashSet();
      Key[] var5 = ANNOTATION_ATTRIBUTES;
      int var6 = var5.length;

      for(int var7 = 0; var7 < var6; ++var7) {
         Key key = var5[var7];
         StructAnnotationAttribute attribute = (StructAnnotationAttribute)mb.getAttribute(key);
         if (attribute != null) {
            Iterator var10 = attribute.getAnnotations().iterator();

            while(var10.hasNext()) {
               AnnotationExprent annotation = (AnnotationExprent)var10.next();
               String text = annotation.toJava(indent, BytecodeMappingTracer.DUMMY).toString();
               filter.add(text);
               buffer.append(text).appendLineSeparator();
            }
         }
      }

      appendTypeAnnotations(buffer, indent, mb, targetType, -1, filter);
   }

   private static void appendParameterAnnotations(TextBuffer buffer, StructMethod mt, int param) {
      Set filter = new HashSet();
      Key[] var4 = PARAMETER_ANNOTATION_ATTRIBUTES;
      int var5 = var4.length;

      for(int var6 = 0; var6 < var5; ++var6) {
         Key key = var4[var6];
         StructAnnotationParameterAttribute attribute = (StructAnnotationParameterAttribute)mt.getAttribute(key);
         if (attribute != null) {
            List annotations = attribute.getParamAnnotations();
            if (param < annotations.size()) {
               Iterator var10 = ((List)annotations.get(param)).iterator();

               while(var10.hasNext()) {
                  AnnotationExprent annotation = (AnnotationExprent)var10.next();
                  String text = annotation.toJava(-1, BytecodeMappingTracer.DUMMY).toString();
                  filter.add(text);
                  buffer.append(text).append(' ');
               }
            }
         }
      }

      appendTypeAnnotations(buffer, -1, mt, 22, param, filter);
   }

   private static void appendTypeAnnotations(TextBuffer buffer, int indent, StructMember mb, int targetType, int index, Set filter) {
      Key[] var6 = TYPE_ANNOTATION_ATTRIBUTES;
      int var7 = var6.length;

      label46:
      for(int var8 = 0; var8 < var7; ++var8) {
         Key key = var6[var8];
         StructTypeAnnotationAttribute attribute = (StructTypeAnnotationAttribute)mb.getAttribute(key);
         if (attribute != null) {
            Iterator var11 = attribute.getAnnotations().iterator();

            while(true) {
               TypeAnnotation annotation;
               do {
                  do {
                     do {
                        if (!var11.hasNext()) {
                           continue label46;
                        }

                        annotation = (TypeAnnotation)var11.next();
                     } while(!annotation.isTopLevel());
                  } while(annotation.getTargetType() != targetType);
               } while(index >= 0 && annotation.getIndex() != index);

               String text = annotation.getAnnotation().toJava(indent, BytecodeMappingTracer.DUMMY).toString();
               if (!filter.contains(text)) {
                  buffer.append(text);
                  if (indent < 0) {
                     buffer.append(' ');
                  } else {
                     buffer.appendLineSeparator();
                  }
               }
            }
         }
      }

   }

   private static void appendModifiers(TextBuffer buffer, int flags, int allowed, boolean isInterface, int excluded) {
      flags &= allowed;
      if (!isInterface) {
         excluded = 0;
      }

      Iterator var5 = MODIFIERS.keySet().iterator();

      while(var5.hasNext()) {
         int modifier = (Integer)var5.next();
         if ((flags & modifier) == modifier && (modifier & excluded) == 0) {
            buffer.append((String)MODIFIERS.get(modifier)).append(' ');
         }
      }

   }

   public static GenericClassDescriptor getGenericClassDescriptor(StructClass cl) {
      if (DecompilerContext.getOption("dgs")) {
         StructGenericSignatureAttribute attr = (StructGenericSignatureAttribute)cl.getAttribute(StructGeneralAttribute.ATTRIBUTE_SIGNATURE);
         if (attr != null) {
            return GenericMain.parseClassSignature(attr.getSignature());
         }
      }

      return null;
   }

   public static void appendTypeParameters(TextBuffer buffer, List parameters, List bounds) {
      buffer.append('<');

      for(int i = 0; i < parameters.size(); ++i) {
         if (i > 0) {
            buffer.append(", ");
         }

         buffer.append((String)parameters.get(i));
         List parameterBounds = (List)bounds.get(i);
         if (parameterBounds.size() > 1 || !"java/lang/Object".equals(((GenericType)parameterBounds.get(0)).value)) {
            buffer.append(" extends ");
            buffer.append(GenericMain.getGenericCastTypeName((GenericType)parameterBounds.get(0)));

            for(int j = 1; j < parameterBounds.size(); ++j) {
               buffer.append(" & ");
               buffer.append(GenericMain.getGenericCastTypeName((GenericType)parameterBounds.get(j)));
            }
         }
      }

      buffer.append('>');
   }

   static {
      ANNOTATION_ATTRIBUTES = new Key[]{StructGeneralAttribute.ATTRIBUTE_RUNTIME_VISIBLE_ANNOTATIONS, StructGeneralAttribute.ATTRIBUTE_RUNTIME_INVISIBLE_ANNOTATIONS};
      PARAMETER_ANNOTATION_ATTRIBUTES = new Key[]{StructGeneralAttribute.ATTRIBUTE_RUNTIME_VISIBLE_PARAMETER_ANNOTATIONS, StructGeneralAttribute.ATTRIBUTE_RUNTIME_INVISIBLE_PARAMETER_ANNOTATIONS};
      TYPE_ANNOTATION_ATTRIBUTES = new Key[]{StructGeneralAttribute.ATTRIBUTE_RUNTIME_VISIBLE_TYPE_ANNOTATIONS, StructGeneralAttribute.ATTRIBUTE_RUNTIME_INVISIBLE_TYPE_ANNOTATIONS};
      MODIFIERS = new LinkedHashMap();
      MODIFIERS.put(1, "public");
      MODIFIERS.put(4, "protected");
      MODIFIERS.put(2, "private");
      MODIFIERS.put(1024, "abstract");
      MODIFIERS.put(8, "static");
      MODIFIERS.put(16, "final");
      MODIFIERS.put(2048, "strictfp");
      MODIFIERS.put(128, "transient");
      MODIFIERS.put(64, "volatile");
      MODIFIERS.put(32, "synchronized");
      MODIFIERS.put(256, "native");
   }

   private static enum MType {
      CLASS,
      FIELD,
      METHOD;
   }
}
