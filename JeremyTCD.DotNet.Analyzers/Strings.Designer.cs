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
    }
}
