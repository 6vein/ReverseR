package org.jetbrains.java.decompiler.main;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.Map.Entry;
import org.jetbrains.java.decompiler.code.CodeConstants;
import org.jetbrains.java.decompiler.code.Instruction;
import org.jetbrains.java.decompiler.code.InstructionSequence;
import org.jetbrains.java.decompiler.main.collectors.BytecodeMappingTracer;
import org.jetbrains.java.decompiler.main.collectors.BytecodeSourceMapper;
import org.jetbrains.java.decompiler.main.collectors.ImportCollector;
import org.jetbrains.java.decompiler.main.extern.IIdentifierRenamer;
import org.jetbrains.java.decompiler.main.extern.IFernflowerLogger.Severity;
import org.jetbrains.java.decompiler.main.extern.IIdentifierRenamer.Type;
import org.jetbrains.java.decompiler.main.rels.ClassWrapper;
import org.jetbrains.java.decompiler.main.rels.LambdaProcessor;
import org.jetbrains.java.decompiler.main.rels.NestedClassProcessor;
import org.jetbrains.java.decompiler.main.rels.NestedMemberAccess;
import org.jetbrains.java.decompiler.modules.decompiler.exps.InvocationExprent;
import org.jetbrains.java.decompiler.struct.StructClass;
import org.jetbrains.java.decompiler.struct.StructContext;
import org.jetbrains.java.decompiler.struct.StructMethod;
import org.jetbrains.java.decompiler.struct.attr.StructEnclosingMethodAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructGeneralAttribute;
import org.jetbrains.java.decompiler.struct.attr.StructInnerClassesAttribute;
import org.jetbrains.java.decompiler.struct.consts.ConstantPool;
import org.jetbrains.java.decompiler.struct.gen.VarType;
import org.jetbrains.java.decompiler.util.InterpreterUtil;
import org.jetbrains.java.decompiler.util.TextBuffer;

public class ClassesProcessor implements CodeConstants {
   public static final int AVERAGE_CLASS_SIZE = 16384;
   private final StructContext context;
   private final Map mapRootClasses = new HashMap();

   public ClassesProcessor(StructContext context) {
       this.context = context;
   }

   public void loadClasses(IIdentifierRenamer renamer) {
       Map mapInnerClasses = new HashMap();
      Map mapNestedClassReferences = new HashMap();
      Map mapEnclosingClassReferences = new HashMap();
      Map mapNewSimpleNames = new HashMap();
      boolean bDecompileInner = DecompilerContext.getOption("din");
      boolean verifyAnonymousClasses = DecompilerContext.getOption("vac");
      Iterator var8 = this.context.getClasses().values().iterator();

      while(true) {
         StructClass cl;
         do {
            do {
               if (!var8.hasNext()) {
                  if (bDecompileInner) {
                     var8 = this.mapRootClasses.entrySet().iterator();

                     label127:
                     while(true) {
                        Entry ent;
                        do {
                           if (!var8.hasNext()) {
                              return;
                           }

                           ent = (Entry)var8.next();
                        } while(mapInnerClasses.containsKey(ent.getKey()));

                        Set setVisited = new HashSet();
                        LinkedList stack = new LinkedList();
                        stack.add(ent.getKey());
                        setVisited.add(ent.getKey());

                        while(true) {
                           while(true) {
                              String superClass;
                              ClassesProcessor.ClassNode superNode;
                              Set setNestedClasses;
                              do {
                                 if (stack.isEmpty()) {
                                    continue label127;
                                 }

                                 superClass = (String)stack.removeFirst();
                                 superNode = (ClassesProcessor.ClassNode)this.mapRootClasses.get(superClass);
                                 setNestedClasses = (Set)mapNestedClassReferences.get(superClass);
                              } while(setNestedClasses == null);

                              StructClass scl = superNode.classStruct;
                              StructInnerClassesAttribute inner = (StructInnerClassesAttribute)scl.getAttribute(StructGeneralAttribute.ATTRIBUTE_INNER_CLASSES);
                              if (inner != null && !inner.getEntries().isEmpty()) {
                                 Iterator var33 = inner.getEntries().iterator();

                                 while(var33.hasNext()) {
                                    org.jetbrains.java.decompiler.struct.attr.StructInnerClassesAttribute.Entry entry = (org.jetbrains.java.decompiler.struct.attr.StructInnerClassesAttribute.Entry)var33.next();
                                    String nestedClass = entry.innerName;
                                    if (setNestedClasses.contains(nestedClass) && setVisited.add(nestedClass)) {
                                       ClassesProcessor.ClassNode nestedNode = (ClassesProcessor.ClassNode)this.mapRootClasses.get(nestedClass);
                                       if (nestedNode == null) {
                                          DecompilerContext.getLogger().writeMessage("Nested class " + nestedClass + " missing!", Severity.WARN);
                                       } else {
                                          ClassesProcessor.Inner rec = (ClassesProcessor.Inner)mapInnerClasses.get(nestedClass);
                                          nestedNode.simpleName = rec.simpleName;
                                          nestedNode.type = rec.type;
                                          nestedNode.access = rec.accessFlags;
                                          if (verifyAnonymousClasses && nestedNode.type == 2 && !isAnonymous(nestedNode.classStruct, scl)) {
                                             nestedNode.type = 4;
                                          }

                                          if (nestedNode.type == 2) {
                                             StructClass cl = nestedNode.classStruct;
                                             nestedNode.access &= -9;
                                             int[] interfaces = cl.getInterfaces();
                                             if (interfaces.length > 0) {
                                                nestedNode.anonymousClassType = new VarType(cl.getInterface(0), true);
                                             } else {
                                                nestedNode.anonymousClassType = new VarType(cl.superClass.getString(), true);
                                             }
                                          } else if (nestedNode.type == 4) {
                                             nestedNode.access &= 1040;
                                          }

                                          superNode.nested.add(nestedNode);
                                          nestedNode.parent = superNode;
                                          nestedNode.enclosingClasses.addAll((Collection)mapEnclosingClassReferences.get(nestedClass));
                                          stack.add(nestedClass);
                                       }
                                    }
                                 }
                              } else {
                                 DecompilerContext.getLogger().writeMessage(superClass + " does not contain inner classes!", Severity.WARN);
                              }
                           }
                        }
                     }
                  }

                  return;
               }

               cl = (StructClass)var8.next();
            } while(!cl.isOwn());
         } while(this.mapRootClasses.containsKey(cl.qualifiedName));

         if (bDecompileInner) {
            StructInnerClassesAttribute inner = (StructInnerClassesAttribute)cl.getAttribute(StructGeneralAttribute.ATTRIBUTE_INNER_CLASSES);
            if (inner != null) {
               Iterator var11 = inner.getEntries().iterator();

               label163:
               while(true) {
                  org.jetbrains.java.decompiler.struct.attr.StructInnerClassesAttribute.Entry entry;
                  String innerName;
                  ClassesProcessor.Inner rec;
                  String enclClassName;
                  do {
                     do {
                        do {
                           if (!var11.hasNext()) {
                              break label163;
                           }

                           entry = (org.jetbrains.java.decompiler.struct.attr.StructInnerClassesAttribute.Entry)var11.next();
                           innerName = entry.innerName;
                           String simpleName = entry.simpleName;
                           String savedName = (String)mapNewSimpleNames.get(innerName);
                           if (savedName != null) {
                              simpleName = savedName;
                           } else if (simpleName != null && renamer != null && renamer.toBeRenamed(Type.ELEMENT_CLASS, simpleName, (String)null, (String)null)) {
                              simpleName = renamer.getNextClassName(innerName, simpleName);
                              mapNewSimpleNames.put(innerName, simpleName);
                           }

                           rec = new ClassesProcessor.Inner();
                           rec.simpleName = simpleName;
                           rec.type = entry.simpleNameIdx == 0 ? 2 : (entry.outerNameIdx == 0 ? 4 : 1);
                           rec.accessFlags = entry.accessFlags;
                           enclClassName = entry.outerNameIdx != 0 ? entry.enclosingName : cl.qualifiedName;
                        } while(enclClassName == null);
                     } while(innerName.equals(enclClassName));
                  } while(rec.type == 1 && !innerName.equals(enclClassName + '$' + entry.simpleName));

                  StructClass enclosingClass = (StructClass)this.context.getClasses().get(enclClassName);
                  if (enclosingClass != null && enclosingClass.isOwn()) {
                     ClassesProcessor.Inner existingRec = (ClassesProcessor.Inner)mapInnerClasses.get(innerName);
                     if (existingRec == null) {
                        mapInnerClasses.put(innerName, rec);
                     } else if (!ClassesProcessor.Inner.equal(existingRec, rec)) {
                        String message = "Inconsistent inner class entries for " + innerName + "!";
                        DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
                     }

                     ((Set)mapNestedClassReferences.computeIfAbsent(enclClassName, (k) -> {
                        return new HashSet();
                     })).add(innerName);
                     ((Set)mapEnclosingClassReferences.computeIfAbsent(innerName, (k) -> {
                        return new HashSet();
                     })).add(enclClassName);
                  }
               }
            }
         }

         ClassesProcessor.ClassNode node = new ClassesProcessor.ClassNode(0, cl);
         node.access = cl.getAccessFlags();
         this.mapRootClasses.put(cl.qualifiedName, node);
      }
   }

   private static boolean isAnonymous(StructClass cl, StructClass enclosingCl) {
       int[] interfaces = cl.getInterfaces();
      if (interfaces.length <= 0) {
         if (cl.superClass == null) {
            String message = "Inconsistent anonymous class definition: '" + cl.qualifiedName + "'. Neither interface nor super class defined.";
            DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
            return false;
         }
      } else {
         boolean hasNonTrivialSuperClass = cl.superClass != null && !VarType.VARTYPE_OBJECT.equals(new VarType(cl.superClass.getString(), true));
         if (hasNonTrivialSuperClass || interfaces.length > 1) {
            String message = "Inconsistent anonymous class definition: '" + cl.qualifiedName + "'. Multiple interfaces and/or super class defined.";
            DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
            return false;
         }
      }

      ConstantPool pool = enclosingCl.getPool();
      int refCounter = 0;
      boolean refNotNew = false;
      StructEnclosingMethodAttribute attribute = (StructEnclosingMethodAttribute)cl.getAttribute(StructGeneralAttribute.ATTRIBUTE_ENCLOSING_METHOD);
      String enclosingMethod = attribute != null ? attribute.getMethodName() : null;
      Iterator var8 = enclosingCl.getMethods().iterator();

      do {
         StructMethod mt;
         do {
            if (!var8.hasNext()) {
               return true;
            }

            mt = (StructMethod)var8.next();
         } while(enclosingMethod != null && !enclosingMethod.equals(mt.getName()));

         try {
            mt.expandData();
            InstructionSequence seq = mt.getInstructionSequence();
            if (seq != null) {
               int len = seq.length();

               for(int i = 0; i < len; ++i) {
                  Instruction instr = seq.getInstr(i);
                  switch(instr.opcode) {
                  case 178:
                  case 179:
                     if (cl.qualifiedName.equals(pool.getLinkConstant(instr.operand(0)).classname)) {
                        ++refCounter;
                        refNotNew = true;
                     }
                  case 180:
                  case 181:
                  case 182:
                  case 183:
                  case 184:
                  case 185:
                  case 186:
                  case 188:
                  case 190:
                  case 191:
                  case 194:
                  case 195:
                  case 196:
                  default:
                     break;
                  case 187:
                  case 189:
                  case 197:
                     if (cl.qualifiedName.equals(pool.getPrimitiveConstant(instr.operand(0)).getString())) {
                        ++refCounter;
                     }
                     break;
                  case 192:
                  case 193:
                     if (cl.qualifiedName.equals(pool.getPrimitiveConstant(instr.operand(0)).getString())) {
                        ++refCounter;
                        refNotNew = true;
                     }
                  }
               }
            }

            mt.releaseResources();
         } catch (IOException var14) {
            String message = "Could not read method while checking anonymous class definition: '" + enclosingCl.qualifiedName + "', '" + InterpreterUtil.makeUniqueKey(mt.getName(), mt.getDescriptor()) + "'";
            DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
            return false;
         }
      } while(refCounter <= 1 && !refNotNew);

      String message = "Inconsistent references to the class '" + cl.qualifiedName + "' which is supposed to be anonymous";
      DecompilerContext.getLogger().writeMessage(message, Severity.WARN);
      return false;
   }

   public void writeClass(StructClass cl, TextBuffer buffer) throws IOException {
       ClassesProcessor.ClassNode root = (ClassesProcessor.ClassNode)this.mapRootClasses.get(cl.qualifiedName);
      if (root.type == 0) {
         DecompilerContext.getLogger().startReadingClass(cl.qualifiedName);

         try {
            ImportCollector importCollector = new ImportCollector(root);
            DecompilerContext.startClass(importCollector);
            (new LambdaProcessor()).processClass(root);
            addClassnameToImport(root, importCollector);
            initWrappers(root);
            (new NestedClassProcessor()).processClass(root, root);
            (new NestedMemberAccess()).propagateMemberAccess(root);
            TextBuffer classBuffer = new TextBuffer(16384);
            (new ClassWriter()).classToJava(root, classBuffer, 0, (BytecodeMappingTracer)null);
            int index = cl.qualifiedName.lastIndexOf("/");
            if (index >= 0) {
               String packageName = cl.qualifiedName.substring(0, index).replace('/', '.');
               buffer.append("package ");
               buffer.append(packageName);
               buffer.append(";");
               buffer.appendLineSeparator();
               buffer.appendLineSeparator();
            }

            int import_lines_written = importCollector.writeImports(buffer);
            if (import_lines_written > 0) {
               buffer.appendLineSeparator();
            }

            int offsetLines = buffer.countLines();
            buffer.append(classBuffer);
            if (DecompilerContext.getOption("bsm")) {
               BytecodeSourceMapper mapper = DecompilerContext.getBytecodeSourceMapper();
               mapper.addTotalOffset(offsetLines);
               if (DecompilerContext.getOption("__dump_original_lines__")) {
                  buffer.dumpOriginalLineNumbers(mapper.getOriginalLinesMapping());
               }

               if (DecompilerContext.getOption("__unit_test_mode__")) {
                  buffer.appendLineSeparator();
                  mapper.dumpMapping(buffer, true);
               }
            }
         } finally {
            destroyWrappers(root);
            DecompilerContext.getLogger().endReadingClass();
         }

      }
   }

   private static void initWrappers(ClassesProcessor.ClassNode node) {
       if (node.type != 8) {
         ClassWrapper wrapper = new ClassWrapper(node.classStruct);
         wrapper.init();
         node.wrapper = wrapper;
         Iterator var2 = node.nested.iterator();

         while(var2.hasNext()) {
            ClassesProcessor.ClassNode nd = (ClassesProcessor.ClassNode)var2.next();
            initWrappers(nd);
         }

      }
   }

   private static void addClassnameToImport(ClassesProcessor.ClassNode node, ImportCollector imp) {
       if (node.simpleName != null && node.simpleName.length() > 0) {
         imp.getShortName(node.type == 0 ? node.classStruct.qualifiedName : node.simpleName, false);
      }

      Iterator var2 = node.nested.iterator();

      while(var2.hasNext()) {
         ClassesProcessor.ClassNode nd = (ClassesProcessor.ClassNode)var2.next();
         addClassnameToImport(nd, imp);
      }

   }

   private static void destroyWrappers(ClassesProcessor.ClassNode node) {
       node.wrapper = null;
      node.classStruct.releaseResources();
      Iterator var1 = node.nested.iterator();

      while(var1.hasNext()) {
         ClassesProcessor.ClassNode nd = (ClassesProcessor.ClassNode)var1.next();
         destroyWrappers(nd);
      }

   }

   public Map getMapRootClasses() {
       return this.mapRootClasses;
   }

   public static class ClassNode {
      public static final int CLASS_ROOT = 0;
      public static final int CLASS_MEMBER = 1;
      public static final int CLASS_ANONYMOUS = 2;
      public static final int CLASS_LOCAL = 4;
      public static final int CLASS_LAMBDA = 8;
      public int type;
      public int access;
      public String simpleName;
      public final StructClass classStruct;
      private ClassWrapper wrapper;
      public String enclosingMethod;
      public InvocationExprent superInvocation;
      public final Map mapFieldsToVars = new HashMap();
      public VarType anonymousClassType;
      public final List nested = new ArrayList();
      public final Set enclosingClasses = new HashSet();
      public ClassesProcessor.ClassNode parent;
      public ClassesProcessor.ClassNode.LambdaInformation lambdaInformation;

      public ClassNode(String content_class_name, String content_method_name, String content_method_descriptor, int content_method_invocation_type, String lambda_class_name, String lambda_method_name, String lambda_method_descriptor, StructClass classStruct) {
          this.type = 8;
         this.classStruct = classStruct;
         this.lambdaInformation = new ClassesProcessor.ClassNode.LambdaInformation();
         this.lambdaInformation.method_name = lambda_method_name;
         this.lambdaInformation.method_descriptor = lambda_method_descriptor;
         this.lambdaInformation.content_class_name = content_class_name;
         this.lambdaInformation.content_method_name = content_method_name;
         this.lambdaInformation.content_method_descriptor = content_method_descriptor;
         this.lambdaInformation.content_method_invocation_type = content_method_invocation_type;
         this.lambdaInformation.content_method_key = InterpreterUtil.makeUniqueKey(this.lambdaInformation.content_method_name, this.lambdaInformation.content_method_descriptor);
         this.anonymousClassType = new VarType(lambda_class_name, true);
         boolean is_method_reference = content_class_name != classStruct.qualifiedName;
         if (!is_method_reference) {
            StructMethod mt = classStruct.getMethod(content_method_name, content_method_descriptor);
            is_method_reference = !mt.isSynthetic();
         }

         this.lambdaInformation.is_method_reference = is_method_reference;
         this.lambdaInformation.is_content_method_static = this.lambdaInformation.content_method_invocation_type == 6;
      }

      public ClassNode(int type, StructClass classStruct) {
          this.type = type;
         this.classStruct = classStruct;
         this.simpleName = classStruct.qualifiedName.substring(classStruct.qualifiedName.lastIndexOf(47) + 1);
      }

      public ClassesProcessor.ClassNode getClassNode(String qualifiedName) {
          Iterator var2 = this.nested.iterator();

         ClassesProcessor.ClassNode node;
         do {
            if (!var2.hasNext()) {
               return null;
            }

            node = (ClassesProcessor.ClassNode)var2.next();
         } while(!qualifiedName.equals(node.classStruct.qualifiedName));

         return node;
      }

      public ClassWrapper getWrapper() {
          ClassesProcessor.ClassNode node;
         for(node = this; node.type == 8; node = node.parent) {
         }

         return node.wrapper;
      }

      public static class LambdaInformation {
         public String method_name;
         public String method_descriptor;
         public String content_class_name;
         public String content_method_name;
         public String content_method_descriptor;
         public int content_method_invocation_type;
         public String content_method_key;
         public boolean is_method_reference;
         public boolean is_content_method_static;
      }
   }

   private static class Inner {
      private String simpleName;
      private int type;
      private int accessFlags;

      private Inner() {
       }

      private static boolean equal(ClassesProcessor.Inner o1, ClassesProcessor.Inner o2) {
          return o1.type == o2.type && o1.accessFlags == o2.accessFlags && InterpreterUtil.equalObjects(o1.simpleName, o2.simpleName);
      }

      // $FF: synthetic method
      Inner(Object x0) {
          this();
      }
   }
}
