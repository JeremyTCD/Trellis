﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JeremyTCD.DotNet.Analyzers {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("JeremyTCD.DotNet.Analyzers.Strings", typeof(Strings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Testing.
        /// </summary>
        public static string CategoryName_Testing {
            get {
                return ResourceManager.GetString("CategoryName_Testing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A unit test method&apos;s dummy local variable is not a mock of its type&apos;s interface..
        /// </summary>
        public static string JA1000_Description {
            get {
                return ResourceManager.GetString("JA1000_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dummy local variable should be a mock of its interface..
        /// </summary>
        public static string JA1000_MessageFormat {
            get {
                return ResourceManager.GetString("JA1000_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unit test methods must use interface mocks for dummies..
        /// </summary>
        public static string JA1000_Title {
            get {
                return ResourceManager.GetString("JA1000_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test class&apos;s namespace is incorrectly formatted..
        /// </summary>
        public static string JA1001_Description {
            get {
                return ResourceManager.GetString("JA1001_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test class namespace is not in the format &quot;&lt;NamespaceOfClassUnderTest&gt;.Tests&quot;..
        /// </summary>
        public static string JA1001_MessageFormat {
            get {
                return ResourceManager.GetString("JA1001_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test class namespaces must be correctly formatted..
        /// </summary>
        public static string JA1001_Title {
            get {
                return ResourceManager.GetString("JA1001_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test class&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1002_Description {
            get {
                return ResourceManager.GetString("JA1002_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test class name is not in the format &quot;&lt;ClassUnderTest&gt;&lt;UnitTests|IntegrationTests|EndToEndTests&gt;&quot;..
        /// </summary>
        public static string JA1002_MessageFormat {
            get {
                return ResourceManager.GetString("JA1002_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test class names must be correctly formatted..
        /// </summary>
        public static string JA1002_Title {
            get {
                return ResourceManager.GetString("JA1002_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1003_Description {
            get {
                return ResourceManager.GetString("JA1003_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method name must be of the format &quot;&lt;MethodUnderTest&gt;_&lt;TestDescription&gt;.
        /// </summary>
        public static string JA1003_MessageFormat {
            get {
                return ResourceManager.GetString("JA1003_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method names must be correctly formatted..
        /// </summary>
        public static string JA1003_Title {
            get {
                return ResourceManager.GetString("JA1003_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method mock local variable&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1004_Description {
            get {
                return ResourceManager.GetString("JA1004_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mock local variable name &quot;{0}&quot; is incorrectly formatted. 
        ///- Instances or mocks of the class under test must be named &quot;testSubject&quot;.
        ///- Mocks with behaviours must have names starting with &quot;mock&quot;.
        ///- Mocks that do not have behaviours must have names starting with &quot;dummy&quot;..
        /// </summary>
        public static string JA1004_MessageFormat {
            get {
                return ResourceManager.GetString("JA1004_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method mock local variable names must be correctly formatted..
        /// </summary>
        public static string JA1004_Title {
            get {
                return ResourceManager.GetString("JA1004_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test subject is not instantiated by a valid create method..
        /// </summary>
        public static string JA1005_Description {
            get {
                return ResourceManager.GetString("JA1005_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use a valid create method to instantiate the object..
        /// </summary>
        public static string JA1005_MessageFormat {
            get {
                return ResourceManager.GetString("JA1005_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test subject must be instantiated in a valid create method..
        /// </summary>
        public static string JA1005_Title {
            get {
                return ResourceManager.GetString("JA1005_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test data method&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1006_Description {
            get {
                return ResourceManager.GetString("JA1006_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test data method name must be of the format &quot;&lt;TestMethod&gt;_Data&quot;.
        /// </summary>
        public static string JA1006_MessageFormat {
            get {
                return ResourceManager.GetString("JA1006_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test data method names must be correctly formatted..
        /// </summary>
        public static string JA1006_Title {
            get {
                return ResourceManager.GetString("JA1006_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A documented exception throwing outcome has no matching unit test..
        /// </summary>
        public static string JA1007_Description {
            get {
                return ResourceManager.GetString("JA1007_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exception throwing outcome has no matching unit test..
        /// </summary>
        public static string JA1007_MessageFormat {
            get {
                return ResourceManager.GetString("JA1007_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Documented exception outcomes must have matching unit tests..
        /// </summary>
        public static string JA1007_Title {
            get {
                return ResourceManager.GetString("JA1007_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method that calls Mock&lt;T&gt;.Setup does not call MockRepository.VerifyAll..
        /// </summary>
        public static string JA1008_Description {
            get {
                return ResourceManager.GetString("JA1008_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method must call MockRepository.VerifyAll..
        /// </summary>
        public static string JA1008_MessageFormat {
            get {
                return ResourceManager.GetString("JA1008_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method must call MockRepository.VerifyAll if it calls Mock&lt;T&gt;.Setup..
        /// </summary>
        public static string JA1008_Title {
            get {
                return ResourceManager.GetString("JA1008_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mock&lt;T&gt;() used instead of MockRepository.Create..
        /// </summary>
        public static string JA1009_Description {
            get {
                return ResourceManager.GetString("JA1009_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use MockRepository.Create instead of Mock&lt;T&gt;()..
        /// </summary>
        public static string JA1009_MessageFormat {
            get {
                return ResourceManager.GetString("JA1009_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MockRepository.Create must be used instead of Mock&lt;T&gt;&apos;s constructor..
        /// </summary>
        public static string JA1009_Title {
            get {
                return ResourceManager.GetString("JA1009_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method&apos;s members are incorrectly ordered..
        /// </summary>
        public static string JA1010_Description {
            get {
                return ResourceManager.GetString("JA1010_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method members must be correctly ordered..
        /// </summary>
        public static string JA1010_MessageFormat {
            get {
                return ResourceManager.GetString("JA1010_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method members must be correctly ordered..
        /// </summary>
        public static string JA1010_Title {
            get {
                return ResourceManager.GetString("JA1010_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create end to end test class....
        /// </summary>
        public static string JA1011_CodeFix_Title_CreateEndToEndTestClass {
            get {
                return ResourceManager.GetString("JA1011_CodeFix_Title_CreateEndToEndTestClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create integration test class....
        /// </summary>
        public static string JA1011_CodeFix_Title_CreateIntegrationTestClass {
            get {
                return ResourceManager.GetString("JA1011_CodeFix_Title_CreateIntegrationTestClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create unit test class....
        /// </summary>
        public static string JA1011_CodeFix_Title_CreateUnitTestClass {
            get {
                return ResourceManager.GetString("JA1011_CodeFix_Title_CreateUnitTestClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method has incorrectly named test subject local variables..
        /// </summary>
        public static string JA1012_Description {
            get {
                return ResourceManager.GetString("JA1012_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method test subject local variables must be named &quot;testSubject&quot;. If a method contains more than one such local variable, their names must end with &quot;TestSubject&quot;..
        /// </summary>
        public static string JA1012_MessageFormat {
            get {
                return ResourceManager.GetString("JA1012_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method test subject local variable names must be correctly formatted..
        /// </summary>
        public static string JA1012_Title {
            get {
                return ResourceManager.GetString("JA1012_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test method has incorrectly named result local variables.
        /// </summary>
        public static string JA1013_Description {
            get {
                return ResourceManager.GetString("JA1013_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method result local variables must be named &quot;result&quot;. If a method contains more than one such local variable, their names must end with &quot;Result&quot;..
        /// </summary>
        public static string JA1013_MessageFormat {
            get {
                return ResourceManager.GetString("JA1013_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method result local variable names must be correctly named.
        /// </summary>
        public static string JA1013_Title {
            get {
                return ResourceManager.GetString("JA1013_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A public property or method is not virtual..
        /// </summary>
        public static string JA1100_Description {
            get {
                return ResourceManager.GetString("JA1100_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property or method must be virtual..
        /// </summary>
        public static string JA1100_MessageFormat {
            get {
                return ResourceManager.GetString("JA1100_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Public properties and methods must be virtual..
        /// </summary>
        public static string JA1100_Title {
            get {
                return ResourceManager.GetString("JA1100_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A factory class&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1200_Description {
            get {
                return ResourceManager.GetString("JA1200_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class name is not in format &quot;&lt;ProducedType&gt;Factory&quot; where &lt;ProducedType&gt; implements the interface that the factory produces..
        /// </summary>
        public static string JA1200_MessageFormat {
            get {
                return ResourceManager.GetString("JA1200_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class names must be correctly formatted..
        /// </summary>
        public static string JA1200_Title {
            get {
                return ResourceManager.GetString("JA1200_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A factory interface&apos;s name is incorrectly formatted..
        /// </summary>
        public static string JA1201_Description {
            get {
                return ResourceManager.GetString("JA1201_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory interface name is not in format &quot;&lt;ProducedInterface&gt;Factory&quot;..
        /// </summary>
        public static string JA1201_MessageFormat {
            get {
                return ResourceManager.GetString("JA1201_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory interface names must be correctly formatted..
        /// </summary>
        public static string JA1201_Title {
            get {
                return ResourceManager.GetString("JA1201_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A factory interface does not have any valid create methods..
        /// </summary>
        public static string JA1202_Description {
            get {
                return ResourceManager.GetString("JA1202_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory interface does not have a method named &quot;Create&quot; that returns the produced interface..
        /// </summary>
        public static string JA1202_MessageFormat {
            get {
                return ResourceManager.GetString("JA1202_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory interface must have at least one valid create method..
        /// </summary>
        public static string JA1202_Title {
            get {
                return ResourceManager.GetString("JA1202_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class does not implement a factory interface..
        /// </summary>
        public static string JA1203_Description {
            get {
                return ResourceManager.GetString("JA1203_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class must implement a factory interface..
        /// </summary>
        public static string JA1203_MessageFormat {
            get {
                return ResourceManager.GetString("JA1203_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class must implement factory interface..
        /// </summary>
        public static string JA1203_Title {
            get {
                return ResourceManager.GetString("JA1203_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create factory infrastructure....
        /// </summary>
        public static string JA1204_CodeFix_Title_CreateFactoryInfrastructure {
            get {
                return ResourceManager.GetString("JA1204_CodeFix_Title_CreateFactoryInfrastructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A factory class ceate method does not return produced class..
        /// </summary>
        public static string JA1204_Description {
            get {
                return ResourceManager.GetString("JA1204_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create method must return produced class..
        /// </summary>
        public static string JA1204_MessageFormat {
            get {
                return ResourceManager.GetString("JA1204_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Factory class create methods must return produced class..
        /// </summary>
        public static string JA1204_Title {
            get {
                return ResourceManager.GetString("JA1204_Title", resourceCulture);
            }
        }
    }
}
