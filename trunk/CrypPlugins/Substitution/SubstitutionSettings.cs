/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Substitution
{
    public class SubstitutionSettings : ISettings
    {
        #region Public Caesar specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Substitution plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void SubstitutionLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        /// <summary>
        /// An enumaration fot the different key variante modes of refilling the cipher alphabet
        /// </summary>
        public enum KeyVariantMode { RestCharAscending = 0, RestCharDescending = 1, FixKeyAtbash = 2 };
        
        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event SubstitutionLogMessage LogMessage;

        /// <summary>
        /// Retrieves the current settings whether the alphabet shoudl be treated as case sensitive or not
        /// </summary>
        [PropertySaveOrder(0)]
        public bool CaseSensitiveAlphabet
        {
            get
            {
                if (caseSensitiveAlphabet == 0) return false;
                else return true;
            }
            set { } //read only
        }

        /// <summary>
        /// Return true if some settings habe been changed. This value should be set externally to false e.g.
        /// when a project was saved.
        /// </summary>
        [PropertySaveOrder(1)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region Private variables

        private bool hasChanges;
        private int selectedAction = 0;
        private KeyVariantMode keyVariant = KeyVariantMode.RestCharAscending;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string cipherAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string keyValue;
        private UnknownSymbolHandlingMode unknowSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private int caseSensitiveAlphabet = 0; //0=case insensitive, 1 = case sensitive

        #endregion

        #region Private methods

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (value[i] == value[j] || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        LogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }

        private void setCipherAlphabet(string value)
        {
            try
            {
                string a = null;
                bool found;
                switch (keyVariant)
                {
                    case KeyVariantMode.RestCharAscending:
                        a = value;
                        for (int i = 0; i < alphabet.Length; i++)
                        {
                            found = false;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (alphabet[i] == a[j])
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                a += alphabet[i];
                        }
                        break;
                    case KeyVariantMode.RestCharDescending:
                        a = value;
                        for (int i = alphabet.Length - 1; i >= 0; i--)
                        {
                            found = false;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (alphabet[i] == a[j])
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                a += alphabet[i];
                        }
                        break;
                    case KeyVariantMode.FixKeyAtbash:
                        a = string.Empty;
                        for (int i = alphabet.Length - 1; i >= 0; i--)
                            a += alphabet[i];
                        break;
                }
                CipherAlphabet = removeEqualChars(a);
                OnPropertyChanged("CipherAlphabet");
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + value + "\"! (" + e.Message + ") Reverting to " + alphabet + "!", NotificationLevel.Error);
                OnPropertyChanged("CipherAlphabet");
            }
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(2)]
        [ContextMenu("Action","Select the Algorithm action", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] {1,2}, "Encrypt","Decrypt")]
        [TaskPane("Action", "Select the Algorithm action", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] {"Encrypt","Decrypt"})]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("Key value (multiple letter)", "Enter one or multiple letters as the key.", null, 2, false, DisplayLevel.Experienced, ControlType.TextBox,"",null)]
        public string KeyValue
        {
            get { return this.keyValue; }
            set 
            { 
                this.keyValue = value;
                setCipherAlphabet(keyValue);
                OnPropertyChanged("KeyValue");
                OnPropertyChanged("AlphabetSymbols");
            }
            
        }

        [PropertySaveOrder(4)]
        [ContextMenu("Unknown symbol handling","What should be done with ecountered characters at the input which are not in the alphabet?",3,DisplayLevel.Expert,ContextMenuControlType.ComboBox, null, new string[] {"Ignore (leave unmodified)", "Remove","Replace with \'?\'"})]
        [TaskPane("Unknown symbol handling", "What should be done with ecountered characters at the input which are not in the alphabet?", null, 3, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"Ignore (leave unmodified)", "Remove","Replace with \'?\'"})]
        public int UnknownSymbolHandling
        {
            get { return (int)this.unknowSymbolHandling; }
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknowSymbolHandling) HasChanges = true;
                this.unknowSymbolHandling = (UnknownSymbolHandlingMode)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("Alphabet case sensitivity","Should upper and lower case be treated differently? (Should a == A)",4,DisplayLevel.Expert, ContextMenuControlType.ComboBox,null, new string[] {"Case insensitive","Case sensitive"})]
        [TaskPane("Alphabet case sensitivity","Should upper and lower case be treated differently? (Should a == A)", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"Case insensitive","Case sensitive"})]
        public int AlphabetCase
        {
            get { return this.caseSensitiveAlphabet; }
            set
            {
                if (value != caseSensitiveAlphabet) hasChanges = true;
                this.caseSensitiveAlphabet = value;
                if (value == 0)
                {
                    if (alphabet == (upperAlphabet + lowerAlphabet))
                    {
                        alphabet = upperAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + "Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                        setCipherAlphabet(keyValue);
                    }
                }
                else
                {
                    if (alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                        setCipherAlphabet(keyValue);
                    }
                }

                //remove equal characters from the current alphabet
                string a = alphabet;
                alphabet = removeEqualChars(alphabet);

                if (a != alphabet)
                {
                    OnPropertyChanged("AlphabetSymbols");
                    LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                OnPropertyChanged("AlphabetCase");
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("Key variant","Select the key variant for the cipher alphabet",5,DisplayLevel.Expert,ContextMenuControlType.ComboBox,null,new string[] {"Remaining characters are filled in ascending order","Remaining characters are filld in descending order","Atbash (the encryption is using a fixed key)"})]
        [TaskPane("Key variant","Select the key variant for the cipher alphabet", null, 5, false, DisplayLevel.Expert, ControlType.ComboBox,new string[] {"Remaining characters are filled in ascending order","Remaining characters are in descending order","Atbash (the encryption is using a fixed key)"})]
        public int KeyVariant
        {
            get { return (int)this.keyVariant; }
            set
            {
                if ((KeyVariantMode)value != keyVariant) HasChanges = true;
                this.keyVariant = (KeyVariantMode)value;
                setCipherAlphabet(keyValue);
                OnPropertyChanged("KeyVariant");
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("Alphabet","This is the used alphabet.", null, 5, false, DisplayLevel.Expert,ControlType.TextBox,"")]
        public string AlphabetSymbols
        {
            get { return this.alphabet; }
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) // cannot accept empty alphabets
                {
                    LogMessage("Ignoring empty alphabet from user! Using previous alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                else if (!alphabet.Equals(a))
                {
                    HasChanges = true;
                    this.alphabet = a;
                    setCipherAlphabet(keyValue); //re-evaluate if the key value is still within the range
                    LogMessage("Accepted new alphabet from user: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("Cipher alphabet", "This is the used cipher alphabet.", null, 6, false, /*                              Apache License
                           Version 2.0, January 2004
                        http://www.apache.org/licenses/

   TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

   1. Definitions.

      "License" shall mean the terms and conditions for use, reproduction,
      and distribution as defined by Sections 1 through 9 of this document.

      "Licensor" shall mean the copyright owner or entity authorized by
      the copyright owner that is granting the License.

      "Legal Entity" shall mean the union of the acting entity and all
      other entities that control, are controlled by, or are under common
      control with that entity. For the purposes of this definition,
      "control" means (i) the power, direct or indirect, to cause the
      direction or management of such entity, whether by contract or
      otherwise, or (ii) ownership of fifty percent (50%) or more of the
      outstanding shares, or (iii) beneficial ownership of such entity.

      "You" (or "Your") shall mean an individual or Legal Entity
      exercising permissions granted by this License.

      "Source" form shall mean the preferred form for making modifications,
      including but not limited to software source code, documentation
      source, and configuration files.

      "Object" form shall mean any form resulting from mechanical
      transformation or translation of a Source form, including but
      not limited to compiled object code, generated documentation,
      and conversions to other media types.

      "Work" shall mean the work of authorship, whether in Source or
      Object form, made available under the License, as indicated by a
      copyright notice that is included in or attached to the work
      (an example is provided in the Appendix below).

      "Derivative Works" shall mean any work, whether in Source or Object
      form, that is based on (or derived from) the Work and for which the
      editorial revisions, annotations, elaborations, or other modifications
      represent, as a whole, an original work of authorship. For the purposes
      of this License, Derivative Works shall not include works that remain
      separable from, or merely link (or bind by name) to the interfaces of,
      the Work and Derivative Works thereof.

      "Contribution" shall mean any work of authorship, including
      the original version of the Work and any modifications or additions
      to that Work or Derivative Works thereof, that is intentionally
      submitted to Licensor for inclusion in the Work by the copyright owner
      or by an individual or Legal Entity authorized to submit on behalf of
      the copyright owner. For the purposes of this definition, "submitted"
      means any form of electronic, verbal, or written communication sent
      to the Licensor or its representatives, including but not limited to
      communication on electronic mailing lists, source code control systems,
      and issue tracking systems that are managed by, or on behalf of, the
      Licensor for the purpose of discussing and improving the Work, but
      excluding communication that is conspicuously marked or otherwise
      designated in writing by the copyright owner as "Not a Contribution."

      "Contributor" shall mean Licensor and any individual or Legal Entity
      on behalf of whom a Contribution has been received by Licensor and
      subsequently incorporated within the Work.

   2. Grant of Copyright License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      copyright license to reproduce, prepare Derivative Works of,
      publicly display, publicly perform, sublicense, and distribute the
      Work and such Derivative Works in Source or Object form.

   3. Grant of Patent License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      (except as stated in this section) patent license to make, have made,
      use, offer to sell, sell, import, and otherwise transfer the Work,
      where such license applies only to those patent claims licensable
      by such Contributor that are necessarily infringed by their
      Contribution(s) alone or by combination of their Contribution(s)
      with the Work to which such Contribution(s) was submitted. If You
      institute patent litigation against any entity (including a
      cross-claim or counterclaim in a lawsuit) alleging that the Work
      or a Contribution incorporated within the Work constitutes direct
      or contributory patent infringement, then any patent licenses
      granted to You under this License for that Work shall terminate
      as of the date such litigation is filed.

   4. Redistribution. You may reproduce and distribute copies of the
      Work or Derivative Works thereof in any medium, with or without
      modifications, and in Source or Object form, provided that You
      meet the following conditions:

      (a) You must give any other recipients of the Work or
          Derivative Works a copy of this License; and

      (b) You must cause any modified files to carry prominent notices
          stating that You changed the files; and

      (c) You must retain, in the Source form of any Derivative Works
          that You distribute, all copyright, patent, trademark, and
          attribution notices from the Source form of the Work,
          excluding those notices that do not pertain to any part of
          the Derivative Works; and

      (d) If the Work includes a "NOTICE" text file as part of its
          distribution, then any Derivative Works that You distribute must
          include a readable copy of the attribution notices contained
          within such NOTICE file, excluding those notices that do not
          pertain to any part of the Derivative Works, in at least one
          of the following places: within a NOTICE text file distributed
          as part of the Derivative Works; within the Source form or
          documentation, if provided along with the Derivative Works; or,
          within a display generated by the Derivative Works, if and
          wherever such third-party notices normally appear. The contents
          of the NOTICE file are for informational purposes only and
          do not modify the License. You may add Your own attribution
          notices within Derivative Works that You distribute, alongside
          or as an addendum to the NOTICE text from the Work, provided
          that such additional attribution notices cannot be construed
          as modifying the License.

      You may add Your own copyright statement to Your modifications and
      may provide additional or different license terms and conditions
      for use, reproduction, or distribution of Your modifications, or
      for any such Derivative Works as a whole, provided Your use,
      reproduction, and distribution of the Work otherwise complies with
      the conditions stated in this License.

   5. Submission of Contributions. Unless You explicitly state otherwise,
      any Contribution intentionally submitted for inclusion in the Work
      by You to the Licensor shall be under the terms and conditions of
      this License, without any additional terms or conditions.
      Notwithstanding the above, nothing herein shall supersede or modify
      the terms of any separate license agreement you may have executed
      with Licensor regarding such Contributions.

   6. Trademarks. This License does not grant permission to use the trade
      names, trademarks, service marks, or product names of the Licensor,
      except as required for reasonable and customary use in describing the
      origin of the Work and reproducing the content of the NOTICE file.

   7. Disclaimer of Warranty. Unless required by applicable law or
      agreed to in writing, Licensor provides the Work (and each
      Contributor provides its Contributions) on an "AS IS" BASIS,
      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
      implied, including, without limitation, any warranties or conditions
      of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
      PARTICULAR PURPOSE. You are solely responsible for determining the
      appropriateness of using or redistributing the Work and assume any
      risks associated with Your exercise of permissions under this License.

   8. Limitation of Liability. In no event and under no legal theory,
      whether in tort (including negligence), contract, or otherwise,
      unless required by applicable law (such as deliberate and grossly
      negligent acts) or agreed to in writing, shall any Contributor be
      liable to You for damages, including any direct, indirect, special,
      incidental, or consequential damages of any character arising as a
      result of this License or out of the use or inability to use the
      Work (including but not limited to damages for loss of goodwill,
      work stoppage, computer failure or malfunction, or any and all
      other commercial damages or losses), even if such Contributor
      has been advised of the possibility of such damages.

   9. Accepting Warranty or Additional Liability. While redistributing
      the Work or Derivative Works thereof, You may choose to offer,
      and charge a fee for, acceptance of support, warranty, indemnity,
      or other liability obligations and/or rights consistent with this
      License. However, in accepting such obligations, You may act only
      on Your own behalf and on Your sole responsibility, not on behalf
      of any other Contributor, and only if You agree to indemnify,
      defend, and hold each Contributor harmless for any liability
      incurred by, or claims asserted against, such Contributor by reason
      of your accepting any such warranty or additional liability.

   END OF TERMS AND CONDITIONS

   APPENDIX: How to apply the Apache License to your work.

      To apply the Apache License to your work, attach the following
      boilerplate notice, with the fields enclosed by brackets "[]"
      replaced with your own identifying information. (Don't include
      the brackets!)  The text should be enclosed in the appropriate
      comment syntax for the file format. We also recommend that a
      file or class name and description of purpose be included on the
      same "printed page" as the copyright notice for easier
      identification within third-party archives.

   Copyright [2008] [Sebastian Przybylski, University of Siegen]

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

DisplayLevel.Expert, ControlType.TextBox, "")]
        public string CipherAlphabet
        {
            get { return this.cipherAlphabet; }
            set 
            {
                this.cipherAlphabet = value;
                OnPropertyChanged("CipherAlphabet");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
