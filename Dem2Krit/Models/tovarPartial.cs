using System;
using System.Globalization;

namespace Dem2Krit.Models
{
    public partial class tovar
    {
        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(foto))
                {
                    return "/Resources/picture.png";
                }
                string fileName = foto.Trim();
                if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".jpg";
                }
                return $"/Resources/{fileName}";
            }
        }
    

    public decimal OriginalPrice
        {
            get
            {
                if (decimal.TryParse(chena, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                    return price;
                if (decimal.TryParse(chena, NumberStyles.Any, new CultureInfo("ru-RU"), out price))
                    return price;
                return 0;
            }
        }

        public decimal FinalPrice
        {
            get
            {
                if (skidka > 0)
                {
                    return Math.Round(OriginalPrice * (1 - (decimal)skidka / 100), 2);
                }
                return OriginalPrice;
            }
        }

        public bool HasDiscount => skidka > 0;

        public string ItemBackgroundColor
        {
            get
            {
                if (kolichestvo_na_sklade == 0)
                {
                    return "#ADD8E6";
                }
                if (skidka > 15)
                {
                    return "#2E8B57";
                }
                return "White";
            }
        }
    }
}