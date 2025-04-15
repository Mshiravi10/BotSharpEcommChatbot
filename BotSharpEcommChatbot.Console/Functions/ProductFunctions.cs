using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;


namespace BotSharpEcommChatbot.Console.Functions
{
    public class ProductFunctions
    {
        private readonly Dictionary<string, string> _products = new()
        {
            ["Shoes"] = "Black sports shoes, $350",
            ["Coat"] = "Navy formal coat, $750",
            ["Phone"] = "XY model Z5 phone with 128GB, $8.5 million"
        };

        [KernelFunction, Description("Retrieve information of a store product using its name")]
        public string GetProductInfo(
            [Description("Product name such as Shoes, Coat, or Phone")] string productName)
        {
            return _products.TryGetValue(productName.Trim(), out var info)
                ? info
                : $"Sorry, the product '{productName}' is not in the list.";
        }
    }
}
