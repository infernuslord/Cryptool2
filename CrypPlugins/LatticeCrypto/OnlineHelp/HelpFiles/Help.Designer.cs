﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.269
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PKCS1.OnlineHelp.HelpFiles {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Help {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Help() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PKCS1.OnlineHelp.HelpFiles.Help", typeof(Help).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
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
        ///   Sucht eine lokalisierte Zeichenfolge, die Zurück ähnelt.
        /// </summary>
        public static string btnBack {
            get {
                return ResourceManager.GetString("btnBack", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Schließen ähnelt.
        /// </summary>
        public static string btnClose {
            get {
                return ResourceManager.GetString("btnClose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Vorwärts ähnelt.
        /// </summary>
        public static string btnForward {
            get {
                return ResourceManager.GetString("btnForward", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Bitposition des Datenblocks&lt;/h2&gt;
        ///Beim &lt;a href=&quot;help://Gen_Bleichenb_Sig_Tab&quot;&gt;Bleichenbacher-Angriff&lt;/a&gt; wird der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; an eine bestimmte Position &quot;verschoben&quot;. 
        ///Die Position in Bit kann hier ausgewählt werden. Die Angabe bezieht sich auf den Anfang des Datenblocks, von rechts gezählt.
        /// ähnelt.
        /// </summary>
        public static string Gen_Bleichenb_BitPos {
            get {
                return ResourceManager.GetString("Gen_Bleichenb_BitPos", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Zeichen zur Nachrichtenänderung&lt;/h2&gt;
        ///Im Rahmen der &lt;a href=&quot;help://Gen_Bleichenb_Sig_Tab&quot;&gt;Signaturerzeugung nach Bleichenbacher&lt;/a&gt;, muss unter Umständen die Ausgangsnachricht verändert werden. 
        ///In dieser Implementierung wird ein einzelnes Zeichen angehangen. Dieses Zeichen kann hier eingegeben werden.
        /// ähnelt.
        /// </summary>
        public static string Gen_Bleichenb_ChangeSign {
            get {
                return ResourceManager.GetString("Gen_Bleichenb_ChangeSign", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Bleichenbacher-Signatur generieren&lt;/h2&gt;
        ///In diesem Tab wird eine gefälschte Signatur generiert, die in ver- und entschlüsselter Form dargestellt wird. 
        ///Eine Signatur, die von fehlerhaften Implementierungen als valide erkannt wird, hat folgende Struktur: 
        ///&apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HD GG. &lt;/br&gt;
        ///Im Einzelnen bedeutet dies:
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;00&apos;&lt;/strong&gt; 
        ///Einleitender Nullblock (8 Bit). Dadurch wird gewährleistet dass, der numerische Wert der Signatur kleiner ist als das 
        ///&lt;a href=&quot;help://KeyGen_ModulusS [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string Gen_Bleichenb_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_Bleichenb_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Datenblock generieren&lt;/h2&gt;
        ///In diesem Tab kann der Datenblock einer Signatur generiert werden. Der Datenblock besteht aus den zwei Teilen &quot;Hashfunction-Identifier&quot; und &quot;Hashdigest&quot; (Hashwert).
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;Hashfunction-Identifier&lt;br /&gt;&lt;/strong&gt;
        ///Der Hashfunction-Identifier ist ein ASN.1-codierter Datenblock, der unter anderem Informationen wie den Namen der verwendeten Hashfunktion (Algorithmidentifier), die Länge des gesamten Datenblocks, und die Länge des Hashwertes beinhaltet.&lt;/br&gt;
        ///Die Länge [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string Gen_Datablock_Tab {
            get {
                return ResourceManager.GetString("Gen_Datablock_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Maximale Anzahl an Iterationen&lt;/h2&gt;
        ///In diesem Textfeld können Sie angeben, wieviele Iterationen der Algorithmus durchlaufen soll, bevor dieser unterbrochen wird. 
        ///Kommt der Algorithmus vorher zu einem Ergebnis endet der Durchlauf und es wird das Ergebnis präsentiert. &lt;/br&gt;
        ///Im Durchschnitt sollte der Algorithmus ca. 131.072 Iterationen brauchen (wenn SHA-1 als Hashalgorithmus gewählt wurde). ähnelt.
        /// </summary>
        public static string Gen_Kuehn_Iterations {
            get {
                return ResourceManager.GetString("Gen_Kuehn_Iterations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Kühn-Signatur generieren&lt;/h2&gt;
        ///In diesem Tab können gefälschte Signaturen, nach der Methode wie sie Ulrich Kühn beschrieben hat, erstellt werden.
        ///Die Signaturen ähneln in der Struktur denen von &lt;a href=&quot;help://Gen_Bleichenb_Sig_Tab&quot;&gt;Bleichenbacher&lt;/a&gt;, machen sich jedoch die Rechenkraft von
        ///Computern zu nutze und sind auch auf Signaturen von 1024 Bit Länge anwendbar. Auch hier liegt folgende Struktur zugrunde: &apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HD GG. &lt;/br&gt;
        ///
        ///Die Unterschiede zu den Bleichenbacher-Signaturen sind  [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string Gen_Kuehn_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_Kuehn_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Signatur generieren&lt;/h2&gt;
        ///In diesem Tab wird die komplette PKCS#1-Signatur erstellt. Die Signatur hat folgende Struktur: &apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HD. &lt;/br&gt;
        ///Im Einzelnen bedeutet dies:
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;00&apos;&lt;/strong&gt; 
        ///Einleitender Nullblock (8 Bit). Dadurch wird gewährleistet dass der numerische Wert der Signatur kleiner ist als das 
        ///&lt;a href=&quot;help://KeyGen_ModulusSize&quot;&gt;RSA-Modul.&lt;/a&gt;
        ///&lt;/li&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;01&apos;&lt;/strong&gt;
        ///Block Type. Dieser Block gibt an, ob es sich um eine Operation mit dem privaten ode [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string Gen_PKCS1_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_PKCS1_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;RSA-Schlüsselgenerierung&lt;/h2&gt;
        ///Um PKCS#1-Signaturen erzeugen und validieren zu können, ist ein RSA-Schlüsselpaar notwendig. Dieses besteht aus einem privaten und einem öffentlichen Schlüssel, sowie einem sog. RSA-Modul, der bei beiden Schlüsseln gleich ist.&lt;/br&gt;
        ///Ein RSA-Schlüssel kann entweder &lt;a href=&quot;help://KeyGen_Tab&quot;&gt;erzeugt&lt;/a&gt;, oder &lt;a href=&quot;help://KeyInput_Tab&quot;&gt;eingegeben&lt;/a&gt; werden.
        ///                                   &lt;/br&gt;&lt;/br&gt;
        ///&lt;strong&gt;Es ist notwendig, zuerst einen Schlüssel zu erzeugen, bev [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string KeyGen {
            get {
                return ResourceManager.GetString("KeyGen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;RSA-Modul&lt;/h2&gt;
        ///Der Modul ist Teil des öffentlichen RSA-Schlüssels. Der Modul wird auch bei der Operation mit dem privaten Schlüssel gebraucht.&lt;/br&gt;
        ///
        ///Da für die Angriffe auf die PKCS#1-Signaturen nicht der Wert des Modul, sondern nur seine Länge in Bits benötigt wird, kann diese hier angegeben werden.
        ///Beim Bleichenbacher-Angriff wurde von einem Modul mit einer Länge von 3072 Bits ausgegangen. Für die Angriffe mit kürzeren Schlüsseln kann hier die Schlüssellänge reduziert werden.
        /// ähnelt.
        /// </summary>
        public static string KeyGen_ModulusSize {
            get {
                return ResourceManager.GetString("KeyGen_ModulusSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;RSA öffentlicher Schlüssel&lt;/h2&gt;
        ///Der öffentliche Schlüssel (public key) des RSA-Schlüsselpaares wird genutzt, um die mit dem privaten Schlüssel 
        ///erstellten Signaturen zu validieren. Aus Performance-Gründen wird gewöhnlich ein öffentlicher Schlüssel mit einem geringen
        ///Hamming-Gewicht genutzt (z.B. 3, 17 oder 65537). Voraussetzung für den Bleichenbacher-Angriff ist, dass der
        ///öffentliche Schlüssel drei ist.
        /// ähnelt.
        /// </summary>
        public static string KeyGen_PubExponent {
            get {
                return ResourceManager.GetString("KeyGen_PubExponent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;RSA-Schlüsselgenerierung&lt;/h2&gt;
        ///Um PKCS#1-Signaturen erzeugen und validieren zu können, ist ein RSA-Schlüsselpaar notwendig. Dieses besteht aus einem privaten und einem öffentlichen Schlüssel, sowie einem sog. RSA-Modul, der bei beiden Schlüsseln gleich ist.&lt;/br&gt;
        ///Für die hier dargestellten Angriffe auf die PKCS#1-Signaturen sind der Wert des öffentlichen Schlüssels und die Länge des Moduls (in Bit) wichtig. Diese Parameter können hier konfiguriert werden. Der öffentliche Schlüssel sowie der Modul werden [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string KeyGen_Tab {
            get {
                return ResourceManager.GetString("KeyGen_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;RSA-Schlüsseleingabe&lt;/h2&gt;
        ///Um PKCS#1-Signaturen erzeugen und validieren zu können, ist ein RSA-Schlüsselpaar notwendig. Dieses besteht aus einem privaten und einem öffentlichen Schlüssel, sowie einem sog. RSA-Modul, der bei beiden Schlüsseln gleich ist.&lt;/br&gt;
        /// Diese Parameter können hier eingegeben werden. 
        ///&lt;/br&gt;&lt;/br&gt;
        ///&lt;strong&gt;Es ist notwendig, zuerst einen Schlüssel zu erzeugen, bevor man die Signaturen generieren kann!&lt;/strong&gt;&quot; ähnelt.
        /// </summary>
        public static string KeyInput_Tab {
            get {
                return ResourceManager.GetString("KeyInput_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;PKCS#1-Signaturgenerierung&lt;/h2&gt;
        ///&lt;strong&gt;Um PKCS#1-Signaturen erzeugen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske
        /// erzeugt werden&lt;/strong&gt;&lt;/br&gt;
        /// &lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        /// &lt;/br&gt;&lt;/br&gt;
        ///Die PKCS#1-Signaturen basieren auf dem asymmetrischen Verschlüsselungsalgorithmus RSA. Daher ist es notwendig, einen
        ///RSA-Schlüssel zu erzeugen.&lt;/br&gt;
        ///Um eine PKCS#1-Signatur zu erzeugen, wird zunächst der [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string SigGen {
            get {
                return ResourceManager.GetString("SigGen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Bleichenbacher-Angriff&lt;/h2&gt;
        ///&lt;strong&gt;Um den Bleichenbacher-Angriff durchführen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske erzeugt werden.&lt;/strong&gt;&lt;/br&gt;
        ///&lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        /// &lt;/br&gt;&lt;/br&gt;
        /// Um eine gefälschte Signatur zu erzeugen, wird zunächst der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; wie in
        /// einer regulären PKCS#1-Signatur generiert. Allerdings unterscheidet sich die &lt;a  [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string SigGenFakeBleichenbacher {
            get {
                return ResourceManager.GetString("SigGenFakeBleichenbacher", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Angriff mit kürzeren Schlüsseln (Kühn)&lt;/h2&gt;
        ///&lt;strong&gt;Um den Angriff mit kürzeren Schlüsseln durchführen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske erzeugt werden.&lt;/strong&gt;&lt;/br&gt;
        ///&lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        ///&lt;/br&gt;
        ///&lt;/br&gt;
        ///Um eine gefälschte Signatur nach der Kühn-Methode zu erzeugen, wird zunächst der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; wie in
        ///einer regulären PKCS#1-Signatur ge [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string SigGenFakeKuehn {
            get {
                return ResourceManager.GetString("SigGenFakeKuehn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;Signaturvalidierung&lt;/h2&gt;
        ///Bei der Validierung einer PKCS#1-Signatur wird eine Operation mit dem öffentlichen Schlüssel durchgeführt.
        ///Das Ergebnis dieser Operation sollte eine Struktur aufweisen, wie &lt;a href=&quot;help://Gen_PKCS1_Sig_Tab&quot;&gt;hier&lt;/a&gt; beschrieben.
        ///Als nächster Schritt wird der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; ausgelesen.&lt;/br&gt;
        ///Dieses Extrahieren des Datenblock kann auf eine korrekte oder auf eine fehlerhafte Art und Weise geschehen. Die fehlerhafte
        ///Implementierung war bis zum [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string SigVal {
            get {
                return ResourceManager.GetString("SigVal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD HTML 4.0 Transitional//EN&quot;&gt;
        ///&lt;html&gt;
        ///	&lt;head&gt;
        ///		&lt;title&gt;&lt;/title&gt;
        ///		&lt;style type=&quot;text/css&quot;&gt;
        ///		  body
        ///		  {
        ///		  	font-family:Arial,Verdana,Georgia;
        ///		  	font-size:smaller;
        ///		  }
        ///		&lt;/style&gt;
        ///	&lt;/head&gt;
        ///	&lt;body&gt;
        ///	&lt;h2&gt;PKCS#1-Signaturen / Bleichenbacher-Angriff&lt;/h2&gt;
        ///	&lt;p align=&quot;justify&quot;&gt;
        ///	PKCS#1-Signaturen basieren auf dem RSA-Verschlüsselungsverfahren. Der Angriff von Daniel Bleichenbacher zielt nicht
        ///	auf das Verschlüsselungsverfahren selbst, sondern auf Implementierung [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string Start {
            get {
                return ResourceManager.GetString("Start", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;h2&gt;PKCS#1 / Bleichenbacher-Angriff - Hilfe&lt;/h2&gt;
        ///Willkommen in der Hilfe des PKCS#1 / Bleichenbacher-Angriff Plugins.&lt;/br&gt;
        ///Hier finden Sie detaillierte Informationen zu PKCS#1-Signaturen und dem Bleichenbacher-Angriff.&lt;/br&gt;&lt;/br&gt;
        ///In die verschiedenen Masken dieses Plugins gelangen Sie mit Hilfe der Navigation auf der linken Seite. In den verschiedenen Masken
        ///wiederum finden Sie mehrere Hilfebuttons. Wenn Sie auf diese klicken, bekommen Sie detaillierte Informationen über das jeweilige Thema.
        /// ähnelt.
        /// </summary>
        public static string StartControl {
            get {
                return ResourceManager.GetString("StartControl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Onlinehilfe Bleichenbacher-Angriff ähnelt.
        /// </summary>
        public static string title {
            get {
                return ResourceManager.GetString("title", resourceCulture);
            }
        }
    }
}