using System;

namespace Org.BouncyCastle.Asn1.Icao
{
    public abstract class IcaoObjectIdentifiers
    {
        //
        // base id
        //
        const string IdIcao = "1.3.27";

        static readonly DerObjectIdentifier IdIcaoMrtd				= new DerObjectIdentifier(IdIcao + ".1");
        static readonly DerObjectIdentifier IdIcaoMrtdSecurity		= new DerObjectIdentifier(IdIcaoMrtd + ".1");
        static readonly DerObjectIdentifier IdIcaoLdsSecurityObject	= new DerObjectIdentifier(IdIcaoMrtdSecurity + ".1");
    }
}
