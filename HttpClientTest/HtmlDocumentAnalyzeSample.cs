using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace HttpClientTest;

public static class HtmlDocumentAnalyzeSample
{
    public static async Task MainTest()
    {
        await ParseLocalHtmlSample();
    }

    private static async Task iHerbTopSellerProducts()
    {
        var url = "https://www.iherb.com/catalog/topsellers";

        var responseText = await HttpHelper.HttpClient.GetStringAsync(url);
        var htmlParser = new HtmlParser();

        var doc = htmlParser.ParseDocument(responseText);
        var selectors = new[]
        {
            "div.top-sellers-row.panel-container div.best-sellers-row:nth-child(1) div.best-sellers-image > a",
            "div.top-sellers-row.panel-container div.best-sellers-row:nth-child(2) div.best-sellers-image > a"
        };
        var productIdList = new List<int>();
        foreach (var selector in selectors)
        {
            foreach (var node in doc.QuerySelectorAll(selector))
            {
                var link = node.GetAttribute("href");
                Guard.NotNull(link);
                var idx = link.LastIndexOf('/');
                if (idx > 0 && int.TryParse(link[(idx + 1)..], out var id))
                {
                    productIdList.Add(id);
                }
            }
        }

        Console.WriteLine(productIdList.ToJson());
    }

    private static async Task ParseLocalHtmlSample()
    {
        var parser = new HtmlParser();
        var html = """<ul><li><em>California Gold Nutrition</em> <strong>LactoBif Probiotics</strong></li><li>Available in 5, 30 &amp; 100 Billion CFU per Veggie Capsule</li><li>Exclusively using FloraFIT® Probiotics by Danisco®</li><li>8 Active &amp; Clinically Researched Probiotic Strains¹</li><li>Individually Double-foil Blister Sealed</li><li>No Refrigeration Necessary²</li><li>Suitable for Vegetarians</li><li>Formulated to Contain: No Dairy, No Gluten, No GMOs, No Soy</li><li>Produced in a 3rd Party Audited cGMP Registered (Certified) Facility</li><li>iTested Quality Confirmed &amp; 100% Gold Guarantee</li></ul><p><em>California Gold Nutrition</em> <strong>Lactobif Probiotic</strong> formulas contain 8 active and clinically researched probiotic strains (5 Lactobacilli &amp; 3 Bifidobacteria) exclusively using FloraFIT® Probiotics from Danisco®.</p><p>Research suggests the probiotic strains within <strong>Lactobif Probiotic</strong> formulas are extremely resistant to low pH and gastrointestinal conditions (e.g. acid, bile, pepsin and pancreatin), as well as adhere to the intestinal cell lines.¹</p><p><strong>Benefits of Individuality, Double-foil Blister Sealed Probiotics:</strong></p><ul><li>Optimal Barrier to Oxygen, Moisture &amp; Light</li><li>Ensures Product Integrity &amp; Protection</li><li>Tamper Resistant &amp; Tamper Evident</li><li>Shelf Stable &amp; No Refrigeration Necessary²</li><li>Guaranteed Potency²</li></ul><p>Creating homemade fermented foods such as yogurt, kefir, kvass, etc is an art that requires specific probiotic culture starter products. Our LactoBif is not formulated to be used as a starter but <strong>LactoBif Probiotic</strong> capsules can be opened and the powder inside added to smoothies, shakes, etc.</p><p>¹In Vitro studies conducted by probiotic raw material supplier.<br></p><p><br></p><p><strong>iHerb Blog:</strong><br><a href=\"https://www.iherb.com/blog/glowing-skin-smoothie-bowl/252\" target=\"_blank\"></a></p><p><a href=\"https://www.iherb.com/blog/glowing-skin-smoothie-bowl/252\" target=\"_blank\"><strong><u>Glowing Skin Smoothie Bowl</u></strong></a><br><a href=\"https://www.iherb.com/blog/california-gold-nutrition-pomegranate-probiotic-smoothie/297\" target=\"_blank\"></a></p><p><a href=\"https://www.iherb.com/blog/california-gold-nutrition-pomegranate-probiotic-smoothie/297\" target=\"_blank\"><strong><u>California Gold Nutrition Pomegranate Probiotic Smoothie</u></strong></a><br><a href=\"https://www.iherb.com/blog/homemade-facial-mask/286\" target=\"_blank\"></a></p><p><a href=\"https://www.iherb.com/blog/homemade-facial-mask/286\" target=\"_blank\"><strong><u>Homemade Probiotic Yogurt Face Mask</u></strong></a><br><a href=\"https://www.iherb.com/blog/madre-labs-zenbu-meal-replacement-shake-recipes/212\" target=\"_blank\"></a></p><p><a href=\"https://www.iherb.com/blog/madre-labs-zenbu-meal-replacement-shake-recipes/212\" target=\"_blank\"><strong><u>Madre Labs Zenbu Meal Replacement Shake Recipes</u></strong></a></p><p><br></p><p><strong><a href=\"https://s3.images-iherb.com/cms/cgn-00965-itested-lactobif30-07-07-2020.pdf\" rel=\"noopener noreferrer\" target=\"_blank\"><u>iTested Verified</u></a></strong></p>""";
        var doc = parser.ParseDocument(html);
        Console.WriteLine(doc.TextContent);
        Console.WriteLine(doc.Text());
        var a = parser.ParseFragment(html, null!);
        Console.WriteLine(a.FirstOrDefault()?.Text());
    }
}
