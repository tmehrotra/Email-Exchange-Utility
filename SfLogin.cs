using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExchangeUtil
{
    class SfLogin
    {
        public SfPartner.SforceService SF_BINDING;
        public ResilientUtils.Resilient_UtilsService NIMBUS_BINDING;

        public SfLogin()
        {
            SF_BINDING = new SfPartner.SforceService();
            NIMBUS_BINDING = new ResilientUtils.Resilient_UtilsService();
        }

        public void login()
        {
            SfPartner.LoginResult lr = SF_BINDING.login(Properties.Settings.Default.SF_UN, Properties.Settings.Default.SF_PW + Properties.Settings.Default.SF_TKN);

            SfPartner.SessionHeader sf_sh = new SfPartner.SessionHeader();
            sf_sh.sessionId = lr.sessionId;
            SF_BINDING.SessionHeaderValue = sf_sh;
            SF_BINDING.Url = lr.serverUrl;

            ResilientUtils.SessionHeader nimbus_sh = new ResilientUtils.SessionHeader();
            nimbus_sh.sessionId = lr.sessionId;
            NIMBUS_BINDING.SessionHeaderValue = nimbus_sh;
        }

        public void logout()
        {
            SF_BINDING.logout();
        }
    }
}
