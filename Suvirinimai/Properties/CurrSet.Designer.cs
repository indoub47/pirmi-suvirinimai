﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18047
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SuvirinimaiApp.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class CurrSet : global::System.Configuration.ApplicationSettingsBase {
        
        private static CurrSet defaultInstance = ((CurrSet)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new CurrSet())));
        
        public static CurrSet Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AktasOutputDirectoryName {
            get {
                return ((string)(this["AktasOutputDirectoryName"]));
            }
            set {
                this["AktasOutputDirectoryName"] = value;
            }
        }
    }
}