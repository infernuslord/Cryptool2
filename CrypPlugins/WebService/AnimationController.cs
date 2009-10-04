using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace WebService
{
    public class AnimationController
    {private WebServicePresentation  presentation;
    private DispatcherTimer controllerTimer;
    private int status, wsSecurityElementsCounter, actualSecurityElementNumber;
   


        public AnimationController(WebServicePresentation presentation)
        {
            this.presentation = presentation;
            controllerTimer = new DispatcherTimer();
            controllerTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            controllerTimer.Tick += new EventHandler(controllerTimer_Tick);

            

        }
        public void initializeAnimation()
        {
            this.presentation.decAnimation.fillEncryptedDataTreeviewElements();
            this.presentation.decAnimation.initializeAnimation();
            if (this.getSecurityElement(0).Equals("ds:Signature"))
            {
                this.status = 1;
            }
            else if (this.getSecurityElement(0).Equals("xenc:EncryptedKey"))
            {
                this.status = 2;
            }
            else
            {
                this.presentation.webService.showWarning("Es sind keine Sicherheitselemente vorhanden");
               
            }
            this.actualSecurityElementNumber = 0;
            presentation.findSignatureItems((TreeViewItem)presentation.soapInput.Items[0], "ds:Signature");

            this.getSecurityTotalNumber();
            this.controllerTimer.Start();
            
        }
        private int getStatus(int actualNumber)
        {
            if (this.getSecurityElement(actualNumber).Equals("ds:Signature"))
            {
                return 1;

            }
            else
            {
                return 2;
            }
        }
        void controllerTimer_Tick(object sender, EventArgs e)
        {
            switch (status)
            {
                case 1:
                    controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this.controllerTimer.Stop();
                    this.presentation.getSignatureTimer().Start();
                    if (actualSecurityElementNumber + 1 < this.wsSecurityElementsCounter)
                    {
                        actualSecurityElementNumber++;
                    }
                    status = this.getStatus(actualSecurityElementNumber);
                    break;
                case 2:
                    controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this.controllerTimer.Stop();
                    this.presentation.decAnimation.getDecryptiontimer().Start();
                    if (actualSecurityElementNumber + 1 < this.wsSecurityElementsCounter)
                    {
                        actualSecurityElementNumber++;
                    }
                    status = this.getStatus(actualSecurityElementNumber);
                    break;
                case 3:
                    this.controllerTimer.Stop();
                    break;

            }
        }
        public void initializeAnimations()
        {
            presentation.decAnimation.initializeAnimation();
            presentation.initializeAnimation();
        }
        public void getSecurityTotalNumber()
        {
            wsSecurityElementsCounter = this.presentation.webService.getValidator().getTotalSecurityElementsNumber();

      
        }
        public string getSecurityElement(int elementNumber)
        {string t=this.presentation.webService.getValidator().getWSSecurityHeaderElement(elementNumber);
        return t;
        }
        public DispatcherTimer getControllerTimer()
        {
            return this.controllerTimer;
        }
    }
}
