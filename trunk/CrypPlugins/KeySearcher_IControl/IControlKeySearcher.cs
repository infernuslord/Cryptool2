using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
// KeyPattern kicked out of this project and will be sourced from the namespace KeySearcher
using KeySearcher.KeyPattern;
using Cryptool.Plugins.KeySearcher_IControl;

namespace Cryptool.PluginBase.Control
{
    public interface IControlKeySearcher : IControl
    {
        /// <summary>
        /// This event gets thrown after Bruteforcing had ended. This is no evidence, that bruteforcing was successful.
        /// But when the returned List is filled, we have (at least a part) of the possible best keys
        /// </summary>
        event KeySearcher_IControl.BruteforcingEnded OnEndedBruteforcing;

        /// <summary>
        /// get the connected Encryption Control
        /// </summary>
        /// <returns></returns>
        IControlEncryption GetEncyptionControl();
        /// <summary>
        /// get the connected CostFunction Control
        /// </summary>
        /// <returns></returns>
        IControlCost GetCostControl();

        /// <summary>
        /// Initiates bruteforcing, but really starts not until all MasterControls had finished initializing.
        /// </summary>
        /// <param name="pattern">a valid KeyPattern to bruteforce</param>
        /// <param name="encryptedData">encryptedData which are necessary for encrypting and calculating a cost factor</param>
        /// <param name="initVector">initVector necessary in some kinds of cipher-block-mode</param>
        void StartBruteforcing(KeyPattern pattern, byte[] encryptedData, byte[] initVector);

        void StopBruteforcing();

        ///// <summary>
        ///// Bruteforces a given KeyPattern with all available cores in multi-threading mode
        ///// </summary>
        ///// <param name="pattern">an arbitrary difficult KeyPattern</param>
        ///// <param name="encryptControl">the encryption type, which will be used for test-wise decryption with the calculated keys</param>
        ///// <param name="costControl">the cost function type, which will be used to evaluate the bruteforcing results</param>
        //void bruteforcePattern(KeyPattern pattern, IControlEncryption encryptControl, IControlCost costControl);
    }
}