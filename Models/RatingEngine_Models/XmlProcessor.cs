using System.Xml.Linq;

namespace DocuBot_Api.Models.RatingEngine_Models
{
    public class XmlProcessor
    {
        public XmlProcessorResult ProcessXml(string xmlContent)
        {
            List<XmlKeyVal> xmlKeyVal = new List<XmlKeyVal>();
            List<XmlTransDetails> xmlTransDetails = new List<XmlTransDetails>();

            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);

                foreach (XElement element in xdoc.Descendants())
                {
                    //if (element.Name.LocalName == "Transaction")
                    //{
                    //    List<string> attvals = element.Attributes().Select(x => x.Value).ToList();
                    //    xmlTransDetails.Add(new XmlTransDetails
                    //    {
                    //        Amount = decimal.Parse(attvals[2]),
                    //        CurrentBalance = decimal.Parse(attvals[3]),
                    //        Mode = attvals[1],
                    //        Narration = attvals[7],
                    //        Reference = attvals[8],
                    //        TransactionTimestamp = attvals[4],
                    //        Txnid = attvals[6],
                    //        TxnType = attvals[0],
                    //        Valuedate = attvals[5]
                    //    });
                    //}
                    if (element.Name.LocalName == "Transaction")
                    {
                        var attributes = element.Attributes().ToDictionary(x => x.Name.LocalName, x => x.Value);

                        xmlTransDetails.Add(new XmlTransDetails
                        {
                            Amount = decimal.Parse(attributes.GetValueOrDefault("amount", "0")),
                            CurrentBalance = decimal.Parse(attributes.GetValueOrDefault("currentBalance", "0")),
                            Mode = attributes.GetValueOrDefault("mode", ""),
                            Narration = attributes.GetValueOrDefault("narration", ""),
                            Reference = attributes.GetValueOrDefault("reference", ""),
                            TransactionTimestamp = attributes.GetValueOrDefault("transactionTimestamp", ""),
                            Txnid = attributes.GetValueOrDefault("txnId", ""),
                            TxnType = attributes.GetValueOrDefault("type", ""),
                            Valuedate = attributes.GetValueOrDefault("valueDate", "")
                        });
                    }
                    else
                    {
                        foreach (var attribute in element.Attributes())
                        {
                            string parentname = attribute.Parent.Name.LocalName;
                            string attributeName = attribute.Name.LocalName;
                            string attributeValue = attribute.Value;
                            xmlKeyVal.Add(new XmlKeyVal { XmlParentNode = parentname, XmlKey = attributeName, XmlValue = attributeValue });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, log, or throw as needed
                Console.WriteLine($"Error processing XML: {ex.Message}");
            }

            return new XmlProcessorResult
            {
                //KeyValues = xmlKeyVal,
                TransactionDetails = xmlTransDetails
            };

        }
    }
}


