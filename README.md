# docusign-repro

### API Issue Reproduction
This is a test ASP.NET WebForms application showing the issue with faxing in the C# API. <https://github.com/docusign/docusign-csharp-client/issues/98>

To see the issue, add the login information for a demo account to `Default.aspx.cs` and then run the application.
You should be redirected to a form with an attachment. Choose to fax the attachment. Then finish signing and
download the cover page.

You will be redirected back to `Default.aspx.cs`, this time with an "event" parameter in the query string.

Expected: event parameter = fax_pending

Observed: event parameter = cancel
