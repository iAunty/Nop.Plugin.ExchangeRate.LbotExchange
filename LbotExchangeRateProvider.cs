using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Plugin.ExchangeRate.LbotExchange
{
    public class LbotExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public LbotExchangeRateProvider(ILocalizationService localizationService,
            ILogger logger)
        {
            this._localizationService = localizationService;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets currency live rates
        /// Rate Url is : https://mybank.landbank.com.tw/Sign/SIGN_finf_01/Index?ERType=Cash
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            if (exchangeRateCurrencyCode == null)
                throw new ArgumentNullException(nameof(exchangeRateCurrencyCode));

            //add twd with rate 1
            var ratesToTwd = new List<Core.Domain.Directory.ExchangeRate>
            {
                new Core.Domain.Directory.ExchangeRate
                {
                    CurrencyCode = "TWD",
                    Rate = 1,
                    UpdatedOn = DateTime.UtcNow
                }
            };

            try
            { 
                using (WebClient wc = new WebClient())
                {
                    var jsonString = wc.DownloadString("https://mybank.landbank.com.tw/SIGN/SIGN_finf_01/GetSCExchangeRates");
                    var rates = JsonConvert.DeserializeObject<LbotJsonObject>(jsonString);
            
                    foreach(var rate in rates.Result)
                    { 
                        ratesToTwd.Add(new Core.Domain.Directory.ExchangeRate()
                        {
                            CurrencyCode = rate.CCY,
                            Rate = 1 / ((Convert.ToDecimal(rate.SpotBuy) + Convert.ToDecimal(rate.SpotSell)) / 2),
                            UpdatedOn = Convert.ToDateTime(rate.QuoDate)
                        });
                    }
                }
            }
            catch(Exception ex)
            { 
                _logger.Error("LBOT exchange rate provider", ex);
            }

            //return result for the twd
            if (exchangeRateCurrencyCode.Equals("twd", StringComparison.InvariantCultureIgnoreCase))
                return ratesToTwd;

            //use only currencies that are supported by Hncb
            var exchangeRateCurrency = ratesToTwd.FirstOrDefault(rate => rate.CurrencyCode.Equals(exchangeRateCurrencyCode, StringComparison.InvariantCultureIgnoreCase));
            if (exchangeRateCurrency == null)
                throw new NopException(_localizationService.GetResource("Plugins.ExchangeRate.LbotExchange.Error"));

            //return result for the selected (not twd) currency
            return ratesToTwd.Select(rate => new Core.Domain.Directory.ExchangeRate
            {
                CurrencyCode = rate.CurrencyCode,
                Rate = Math.Round(rate.Rate / exchangeRateCurrency.Rate, 4),
                UpdatedOn = rate.UpdatedOn
            }).ToList();
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExchangeRate.LbotExchange.Error", "You can use LBOT (Land bank of Taiwan) exchange rate provider only when the primary exchange rate currency is supported by LBOT");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.ExchangeRate.LbotExchange.Error");

            base.Uninstall();
        }

        #endregion

    }
}