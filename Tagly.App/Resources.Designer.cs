﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tagly.App {
    using System;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("Tagly.App.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string SourceLabel {
            get {
                return ResourceManager.GetString("SourceLabel", resourceCulture);
            }
        }
        
        public static string SelectLabel {
            get {
                return ResourceManager.GetString("SelectLabel", resourceCulture);
            }
        }
        
        public static string FinishSelected {
            get {
                return ResourceManager.GetString("FinishSelected", resourceCulture);
            }
        }
        
        public static string LoadFromDirectory {
            get {
                return ResourceManager.GetString("LoadFromDirectory", resourceCulture);
            }
        }
        
        public static string CenterMap {
            get {
                return ResourceManager.GetString("CenterMap", resourceCulture);
            }
        }
        
        public static string Success {
            get {
                return ResourceManager.GetString("Success", resourceCulture);
            }
        }
        
        public static string Failure {
            get {
                return ResourceManager.GetString("Failure", resourceCulture);
            }
        }
        
        public static string SuccessfullyLoaded {
            get {
                return ResourceManager.GetString("SuccessfullyLoaded", resourceCulture);
            }
        }
        
        public static string SuccessfullySent {
            get {
                return ResourceManager.GetString("SuccessfullySent", resourceCulture);
            }
        }
        
        public static string LoginFailure {
            get {
                return ResourceManager.GetString("LoginFailure", resourceCulture);
            }
        }
    }
}
