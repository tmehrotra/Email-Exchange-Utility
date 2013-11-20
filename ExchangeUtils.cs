using System;
using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;
using System.IO;

namespace ExchangeUtil
{
    class ExchangeUtils
    {
        private ExchangeService service = new ExchangeService();
        private static ExtendedPropertyDefinition TextBodyProperty = new ExtendedPropertyDefinition(0x1000, MapiPropertyType.String);
        private static ExtendedPropertyDefinition HtmlBodyProperty = new ExtendedPropertyDefinition(0x1013, MapiPropertyType.Binary);
        private List<Order_Email> emails = new List<Order_Email>();

        public ExchangeUtils(string ex_uri)
        {
            service.Credentials = new WebCredentials(Properties.Settings.Default.EX_UN, Properties.Settings.Default.EX_PW, Properties.Settings.Default.EX_DOM);
        }

        public List<Order_Email> getEmails()
        {
            emails = new List<Order_Email>();

            ItemView view = new ItemView(20, 0, OffsetBasePoint.Beginning)
            {
                PropertySet = new PropertySet(BasePropertySet.IdOnly) {
                    TextBodyProperty
                }
            };

            SearchFilter theSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And ,new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));

            foreach (string thisServerSetting in Properties.Settings.Default.EX_URI) {
                string[] serverSettings = thisServerSetting.Split('|');

                if (serverSettings[0] != null) {
                    service.Url = new Uri(serverSettings[0]);

                   if (serverSettings[1] != null
                        && Boolean.Parse(serverSettings[1]))
                    {
                        Folder inbox = Folder.Bind(service, WellKnownFolderName.Inbox);
                       
                    }

                    if (serverSettings[2] != null) {                                                
                        
                        List<SearchFilter> publicFolderFilters = new List<SearchFilter>();

                        foreach (String thisPublicFolderName in serverSettings[2].Split(';'))
                        {
                            publicFolderFilters.Add(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, thisPublicFolderName));
                        }
                        
                        publicFolderFilters.Add(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "Email-Case-In-API"));

                        if (publicFolderFilters.Count > 0) {

                            FolderView Fview = new FolderView(10);
                            Fview.Traversal = FolderTraversal.Shallow;
                            SearchFilter searchFilter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "inbox");
                            FindFoldersResults findFoldersResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, searchFilter, Fview);
                            
                           // FindFoldersResults findFoldersResults = service.FindFolders(WellKnownFolderName.Inbox, Fview);
                            
                            //foreach (Folder thisPublicFolder in findFoldersResults.Folders)
                            //{
                                //FindFoldersResults fr1 =  thisPublicFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "smartnumbers-Services"), Fview);

                                //foreach (Folder subFldr1 in fr1.Folders)
                                //{
                                    //FindFoldersResults fr2 = subFldr1.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "read"), Fview);
                                   // foreach (Folder subFldr2 in findFoldersResults.Folders)
                                   // {
                            foreach (Folder fld in findFoldersResults.Folders)
                            {
                                searchFolder(emails, fld, theSearchFilter, view);

                                
                            }
                           
                                   // }
                                //}                                
                            //}
                        }
                   } 
                }
            }

            return emails;
        }

        private List<Order_Email> searchFolder(List<Order_Email> emails, Folder thisFolder, SearchFilter theSearchFilter, ItemView view)
        {
            FindItemsResults<Item> findResults;
            FolderView Fview = new FolderView(10);
            Fview.Traversal = FolderTraversal.Shallow;
            Folder readFolder=new Folder(service);
            FindFoldersResults findReadFolderResult = thisFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, "read"), Fview);
            foreach (Folder rdFolder in findReadFolderResult)
            {
                if (rdFolder.DisplayName == "Read")
                {
                    readFolder = rdFolder;
                    
                }
                
            }
            do
            {
                findResults = thisFolder.FindItems(theSearchFilter, view);

                if (findResults.Items.Count > 0)
                {
                    service.LoadPropertiesForItems(findResults, new PropertySet(EmailMessageSchema.Subject,
                                                                                    EmailMessageSchema.Body,
                                                                                    EmailMessageSchema.InReplyTo,
                                                                                    EmailMessageSchema.HasAttachments,
                                                                                    EmailMessageSchema.Attachments,
                                                                                    EmailMessageSchema.From,
                                                                                    EmailMessageSchema.IsRead));

                    foreach (Item findResult in findResults.Items)
                    {
                        if (findResult is EmailMessage)
                        {
                            EmailMessage email = (EmailMessage)findResult;

                            Order_Email orderEmail = new Order_Email();
                            orderEmail.id = email.Id.ToString();
                            orderEmail.subject = email.Subject;
                            orderEmail.body = email.Body;
                            orderEmail.from = email.From.Address;
                            orderEmail.hasAttachment = email.HasAttachments;

                            if (email.HasAttachments)
                            {
                                foreach (Attachment attachment in email.Attachments)
                                {
                                    if (attachment is FileAttachment)
                                    {
                                        FileAttachment thisFileAttachment = (FileAttachment)attachment;

                                        Order_Attachment orderAttachment = new Order_Attachment();
                                        orderAttachment.id = thisFileAttachment.Id;
                                        orderAttachment.name = thisFileAttachment.Name;

                                        MemoryStream fileStream = new MemoryStream();
                                        thisFileAttachment.Load(fileStream);
                                        StreamReader fileStreamReader = new StreamReader(fileStream);
                                        fileStream.Seek(0, SeekOrigin.Begin);

                                        orderAttachment.attachmentBody = fileStreamReader.ReadToEnd();

                                        orderEmail.attachments.Add(orderAttachment);
                                    }
                                }
                            }

                            emails.Add(orderEmail);

                            email.IsRead = true;
                            email.Update(ConflictResolutionMode.AlwaysOverwrite);
                            email.Move(readFolder.Id);
                            
                        }

                        if (findResults.NextPageOffset.HasValue)
                        {
                            view.Offset = findResults.NextPageOffset.Value;
                        }
                    }
                }
            }
            while (findResults.MoreAvailable);

            return emails;
        }
    
        public class Order_Email
        {
            public string id { get; set; }
            public string inreplyto { get; set; }
            public string subject { get; set; }
            public string body { get; set; }
            public string from { get; set; }
            public Boolean hasAttachment { get; set; }
            public List<Order_Attachment> attachments { get; set; }

            public Order_Email()
            {
                attachments = new List<Order_Attachment>();
            }
        }

        public class Order_Attachment
        {
            public string id { get; set; }
            public string name { get; set; }
            public string attachmentBody { get; set; }

            public Order_Attachment()
            {
            }
        }
    }
}
