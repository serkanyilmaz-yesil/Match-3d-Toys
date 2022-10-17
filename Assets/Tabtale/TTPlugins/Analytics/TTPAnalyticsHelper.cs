#if TTP_ANALYTICS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
    public class TTPAnalyticsHelper
    {
        public static IDictionary<string, object> GenerateProductsReceived(IDictionary<string, object>[] items,
                                                             IDictionary<string, object> realCurrency,
                                                             IDictionary<string, object>[] virtualCurrencies)
        {
            Dictionary<string, object> itemDDNA = new Dictionary<string, object>();
            if (items != null)
            {
                itemDDNA.Add("items", items);
            }

            if (realCurrency != null)
            {
                itemDDNA.Add("realCurrency", realCurrency);
            }

            if (virtualCurrencies != null)
            {
                itemDDNA.Add("virtualCurrencies", virtualCurrencies);
            }

            return itemDDNA;
        }
        [Preserve]
        public static IDictionary<string, object> GenerateProductsSpent(IDictionary<string, object>[] items,
                                                             IDictionary<string, object> realCurrency,
                                                             IDictionary<string, object>[] virtualCurrencies)
        {
            return GenerateProductsReceived(items, realCurrency, virtualCurrencies);
        }
        [Preserve]
        public static IDictionary<string, object> GenerateItem(int itemAmount, string itemName, string itemType)
        {
            if (itemName == null || itemType == null)
            {
                Debug.LogError("no itemName or itemType");
                return null;
            }

            IDictionary<string, object> itemDDNA = new Dictionary<string, object>();
            itemDDNA.Add("itemAmount", itemAmount);
            itemDDNA.Add("itemName", itemName);
            itemDDNA.Add("itemType", itemType);

            IDictionary<string, object> itemEncapsulate = new Dictionary<string, object>();
            itemEncapsulate.Add("item", itemDDNA);

            return itemEncapsulate;
        }

        public static IDictionary<string, object> GenerateVirtualCurrency(int virtualCurrencyAmount, string virtualCurrencyName, string virtualCurrencyType)
        {
            if (virtualCurrencyName == null || virtualCurrencyType == null)
            {
                Debug.LogError("no virtualCurrencyName or virtualCurrencyType");
                return null;
            }

            IDictionary<string, object> vcDDNA = new Dictionary<string, object>();
            vcDDNA.Add("virtualCurrencyAmount", virtualCurrencyAmount);
            vcDDNA.Add("virtualCurrencyName", virtualCurrencyName);
            vcDDNA.Add("virtualCurrencyType", virtualCurrencyType);

            IDictionary<string, object> vcEncapsulate = new Dictionary<string, object>();
            vcEncapsulate.Add("virtualCurrency", vcDDNA);

            return vcEncapsulate;
        }
        [Preserve]
        public static IDictionary<string, object> GenerateRealCurrency(string realCurrencyAmount, string realCurrencyType)
        {
            if (realCurrencyType == null)
            {
                Debug.LogError("no realCurrencyType");
                return null;
            }

            decimal dec = System.Convert.ToDecimal(realCurrencyAmount);
            int priceInt = ConvertCurrency(realCurrencyType, dec);

            IDictionary<string, object> grDDNA = new Dictionary<string, object>();
            grDDNA.Add("realCurrencyAmount", priceInt);
            grDDNA.Add("realCurrencyType", realCurrencyType);

            return grDDNA;
        }

        private static int ConvertCurrency(string code, decimal value)
        {
            IDictionary<string, int> ISO4217 = GetISOValue();
            if (ISO4217.ContainsKey(code))
            {
                return decimal.ToInt32(value * (decimal)System.Math.Pow(10, ISO4217[code]));
            }
            else
            {
                Debug.LogWarning("Failed to find currency for: " + code);
                return 0;
            }
        }

        private static IDictionary<string, int> GetISOValue()
        {
            Debug.Log("TTPAnalyticsHelper::GetISOValue: ");
            IDictionary<string, int> ISO4217 = new Dictionary<string, int>();
            string iso4217_file_to_string = TTPUtils.ReadStreamingAssetsFile("iso_4217.xml");
            Debug.Log("TTPAnalyticsHelper::GetISOValue: iso4217_file_to_string length=" + iso4217_file_to_string.Length);
            using (XmlReader reader = XmlReader.Create(new StringReader(iso4217_file_to_string)))
            {
                bool expectingCode = false;
                bool expectingValue = false;
                string pulledCode = null;
                string pulledValue = null;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("Ccy"))
                            {
                                expectingCode = true;
                            }
                            else if (reader.Name.Equals("CcyMnrUnts"))
                            {
                                expectingValue = true;
                            }
                            break;

                        case XmlNodeType.Text:
                            if (expectingCode)
                            {
                                pulledCode = reader.Value;
                            }
                            else if (expectingValue)
                            {
                                pulledValue = reader.Value;
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("Ccy"))
                            {
                                expectingCode = false;
                            }
                            else if (reader.Name.Equals("CcyMnrUnts"))
                            {
                                expectingValue = false;
                            }
                            else if (reader.Name.Equals("CcyNtry"))
                            {
                                if (!string.IsNullOrEmpty(pulledCode)
                                    && !string.IsNullOrEmpty(pulledValue))
                                {
                                    int value;
                                    try
                                    {
                                        value = int.Parse(pulledValue);
                                    }
                                    catch (System.FormatException)
                                    {
                                        value = 0;
                                    }

                                    ISO4217[pulledCode] = value;
                                }

                                expectingCode = false;
                                expectingValue = false;
                                pulledCode = null;
                                pulledValue = null;
                            }
                            break;
                    }
                }
            }

            return ISO4217;
        }
    }
}
#endif