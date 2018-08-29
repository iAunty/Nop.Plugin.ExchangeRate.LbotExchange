using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.ExchangeRate.LbotExchange
{
    public class LbotJsonObject
    {
        public List<LbotCurrencyRate> Result { get; set; }

        public class LbotCurrencyRate
        {
            //貨幣名稱
            public string CCY { get; set; }

            //中文名稱
            public string CCYItem { get; set; }

            //ID
            public string CCYId { get; set; }

            //及時買入
            public string SpotBuy { get; set; }

            //及時賣出
            public string SpotSell { get; set; }

            //現金買入
            public string CashBuy { get; set; }

            //現金賣出
            public string CashSell { get; set; }

            //更新時間
            public string QuoDate { get; set; }
        }
    }
}
