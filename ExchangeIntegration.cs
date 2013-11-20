using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExchangeUtil
{
    class ExchangeIntegration
    {
        public static void run()
        {
            while (true)
            {
                Utils.writeLog(Utils.logLevel.BEGIN, "Processing Emails", null);

                try
                {
                    checkEmails();
                }
                catch (Exception ex)
                {
                    Utils.writeLog(Utils.logLevel.FATAL, ex.Message, ex);
                }

                Utils.writeLog(Utils.logLevel.END, "Finished Processing Emails", null);

                System.Threading.Thread.Sleep(Properties.Settings.Default.WAIT_MS);
            }
        }
        
        private static void checkEmails()
        {
            EspressoRegexUtils regexMatcher = new EspressoRegexUtils();
            regexMatcher.regex_init();

            foreach (string ex_uri in Properties.Settings.Default.EX_URI)
            {
                ExchangeUtils exchUtils = new ExchangeUtils(ex_uri);
                List<ExchangeUtils.Order_Email> emails = exchUtils.getEmails();

                SfLogin sfLogin = new SfLogin();

                try
                {
                    sfLogin.login();

                    foreach (ExchangeUtils.Order_Email email in emails)
                    {
                        SfUtils sfUtils = new SfUtils();
                        try
                        {
                            Utils.writeLog(Utils.logLevel.INFO, "Processing email " + email.from + "/" + email.subject, null);

                            regexMatcher.regex_match(email.body);
                            foreach (ExchangeUtils.Order_Attachment attach in email.attachments)
                            {
                                regexMatcher.regex_match(attach.attachmentBody);
                            }

                            sfUtils.processEmail(sfLogin, email.from, "", regexMatcher.regex_getXML());

                            foreach (ExchangeUtils.Order_Attachment attach in email.attachments)
                            {
                                sfUtils.addAttachment(attach.name, attach.attachmentBody);
                            }

                            sfUtils.addAttachment(email.subject + ".txt", email.body);
                            sfUtils.createAttachments(sfLogin);

                            Utils.writeLog(Utils.logLevel.SUCCESS, sfUtils.OBJ_ID, null);
                        }
                        catch (Exception ex)
                        {
                            Utils.writeLog(Utils.logLevel.FATAL, ex.Message, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.writeLog(Utils.logLevel.FATAL, ex.Message, ex);
                }
                finally
                {
                    sfLogin.logout();
                }
            }
        }
    }
}
