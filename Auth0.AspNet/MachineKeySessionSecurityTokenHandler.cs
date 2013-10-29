namespace Auth0.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web.Security;

    public class MachineKeySessionSecurityTokenHandler : Microsoft.IdentityModel.Tokens.SessionSecurityTokenHandler
    {
        public MachineKeySessionSecurityTokenHandler()
            : base(CreateTransforms())
        { }

        public MachineKeySessionSecurityTokenHandler(Microsoft.IdentityModel.Tokens.SecurityTokenCache cache, TimeSpan tokenLifetime)
            : base(CreateTransforms(), cache, tokenLifetime)
        { }

        private static System.Collections.ObjectModel.ReadOnlyCollection<Microsoft.IdentityModel.Web.CookieTransform> CreateTransforms()
        {
            return new List<Microsoft.IdentityModel.Web.CookieTransform>
                {
                    new Microsoft.IdentityModel.Web.DeflateCookieTransform(),
                    new MachineKeyCookieTransform()
                }.AsReadOnly();
        }

        public class MachineKeyCookieTransform : Microsoft.IdentityModel.Web.CookieTransform
        {
            public override byte[] Decode(byte[] encoded)
            {
                return MachineKey.Decode(Encoding.UTF8.GetString(encoded), MachineKeyProtection.All);
            }

            public override byte[] Encode(byte[] value)
            {
                return Encoding.UTF8.GetBytes(MachineKey.Encode(value, MachineKeyProtection.All));
            }
        }
    }
}
