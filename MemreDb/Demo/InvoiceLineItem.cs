using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class InvoiceLineItem
    {
        internal uint InvoiceLineItemId { get; set; }
        internal uint InvoiceId { get; set; }
        internal string LineItemDescription { get; set; }
        internal uint LineItemQuantity { get; set; }
        internal decimal LineItemUnitCost { get; set; }
        public InvoiceLineItem()
        {
            LineItemDescription = string.Empty;
        }

        public override string ToString()
        {
            return $"{InvoiceLineItemId} = {LineItemDescription} InvoiceId: {InvoiceId} Quantity: {LineItemQuantity} Unit Cost: {LineItemUnitCost}";
        }
    }
}
