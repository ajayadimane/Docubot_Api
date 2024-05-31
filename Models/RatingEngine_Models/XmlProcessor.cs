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
                    if (element.Name.LocalName == "Transaction")
                    {
                        List<string> attvals = element.Attributes().Select(x => x.Value).ToList();
                        xmlTransDetails.Add(new XmlTransDetails
                        {
                            Amount = decimal.Parse(attvals[0]),
                            CurrentBalance = decimal.Parse(attvals[1]),
                            Mode = attvals[2],
                            Narration = attvals[3],
                            Reference = attvals[4],
                            TransactionTimestamp = attvals[5],
                            Txnid = attvals[6],
                            TxnType = attvals[7],
                            Valuedate = attvals[8]
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


