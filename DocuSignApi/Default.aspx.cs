using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using DocuSign.eSign.Client;

namespace DocuSignApi
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private static DocuSign.eSign.Model.LoginInformation LoginInformation { get; set; }

        protected string EventParameterValue { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // API TEST: if docusign has redirected back to here, show the event. otherwise, generate the signer view and redirect to it.
            if (Request["event"] != null)
            {
                EventParameterValue = Request["event"];
                return;
            }

            // API TEST: enter a demo account key, email and password here.
            string integratorKey = "";
            string integratorEmail = "";
            string password = "";
            string sampleName = "John Doe";
            string sampleEmail = "foo@example.com";
            if (integratorKey == null || integratorKey.Length < 1 ||
                integratorEmail == null || integratorEmail.Length < 1 ||
                password == null || password.Length < 1)
            {
                EventParameterValue = "... please add your DocuSign info to the project source";
                return;
            }

            // initialize client for desired environment (for production change to www)
            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;

            // configure 'X-DocuSign-Authentication' header
            string authHeader = "{\"Username\":\"" + integratorEmail + "\", \"Password\":\"" + password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

            // login call is available in the authentication api 
            AuthenticationApi authApi = new AuthenticationApi();
            DocuSign.eSign.Model.LoginInformation LoginInformation = authApi.Login();

            if (LoginInformation == null)
            {
                return;
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(HttpRuntime.AppDomainAppPath + "contractretail.pdf");

            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "RFP Contract Signing Request";

            // Add a document to the envelope
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = "LendingContract.pdf";
            doc.DocumentId = "1";

            envDef.Documents = new List<Document>();
            envDef.Documents.Add(doc);
            envDef.Recipients = new Recipients();

            envDef.Recipients.Signers = new List<Signer>();

            Signer signer = new Signer();
            signer.Email = sampleEmail;
            signer.Name = sampleName;
            signer.ClientUserId = "ABC123";
            signer.RecipientId = "1";

            signer.Tabs = new Tabs();

            signer.Tabs.SignHereTabs = new List<SignHere>();
            SignHere signHere = new SignHere();
            signHere.DocumentId = "1";
            signHere.PageNumber = "5";
            signHere.RecipientId = signer.RecipientId;
            signHere.XPosition = "25";
            signHere.YPosition = "430";
            signer.Tabs.SignHereTabs.Add(signHere);

            signer.Tabs.SignerAttachmentTabs = new List<SignerAttachment>();
            SignerAttachment signerAttachment = new SignerAttachment();
            signerAttachment.DocumentId = "1";
            signerAttachment.PageNumber = "5";
            signerAttachment.RecipientId = signer.RecipientId;
            signerAttachment.XPosition = "325";
            signerAttachment.YPosition = "430";
            signerAttachment.TabLabel = "Invoice";
            signer.Tabs.SignerAttachmentTabs.Add(signerAttachment);

            envDef.Recipients.Signers.Add(signer);

            // set envelope status to "sent" to immediately send the signature request
            envDef.Status = "sent";

            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(LoginInformation.LoginAccounts[0].AccountId, envDef);

            RecipientViewRequest viewRequest = new RecipientViewRequest()
            {
                ReturnUrl = "http://localhost:61158/Default.aspx",
                ClientUserId = "ABC123",
                AuthenticationMethod = "email",
                UserName = sampleName,
                Email = sampleEmail
            };

            EnvelopesApi envelopesApi2 = new EnvelopesApi();
            ViewUrl viewUrl = envelopesApi2.CreateRecipientView(LoginInformation.LoginAccounts[0].AccountId, envelopeSummary.EnvelopeId, viewRequest);

            Response.Redirect(viewUrl.Url);

        }
    }
}