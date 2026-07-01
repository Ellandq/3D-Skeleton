using System.Linq;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Screens.SettingsEnforcement
{
    public class SettingsCoverageResolver
    {
        public static void Refresh()
        {
            SettingsEnforcerScanner.Refresh();
        }


        public CoverageStatus GetPageStatus(SettingsPageSO page)
        {
            var own = FindEnforcer(page.pageName);

            if (own != null)
                return Convert(own);


            if (page.categories.Count <= 0) return CoverageStatus.Red;
            var children =
                page.categories
                    .Select(x =>
                        GetCategoryStatus(page, x))
                    .ToList();


            return children.All(IsCovered) ? Worst(children) : CoverageStatus.Red;
        }



        public CoverageStatus GetCategoryStatus(
            SettingsPageSO page,
            SettingsPageCategorySO category)
        {
            var key =
                $"{page.pageName}/{category.categoryName}";


            var own = FindEnforcer(key);

            if (own != null)
                return Convert(own);


            if (category.items.Count > 0)
            {
                var children =
                    category.items
                        .Select(x =>
                            GetItemStatus(
                                page,
                                category,
                                x))
                        .ToList();


                if (children.All(IsCovered))
                    return Worst(children);
            }


            var parent =
                FindEnforcer(page.pageName);


            return parent != null ? Convert(parent) : CoverageStatus.Red;
        }



        public CoverageStatus GetItemStatus(
            SettingsPageSO page,
            SettingsPageCategorySO category,
            SettingsPageItemSO item)
        {
            var key =
                $"{page.pageName}/{category.categoryName}/{item.settingName}";


            var own = FindEnforcer(key);

            if (own != null)
                return Convert(own);


            var categoryEnforcer =
                FindEnforcer(
                    $"{page.pageName}/{category.categoryName}");


            if (categoryEnforcer != null)
                return Convert(categoryEnforcer);


            var pageEnforcer =
                FindEnforcer(page.pageName);


            return pageEnforcer != null ? Convert(pageEnforcer) : CoverageStatus.Red;
        }



        private SettingsEnforcerInfo FindEnforcer(string key)
        {
            return SettingsEnforcerScanner.Enforcers
                .FirstOrDefault(x =>
                    x.key == key);
        }



        private CoverageStatus Convert(
            SettingsEnforcerInfo info)
        {
            return info.hasImplementation
                ? CoverageStatus.Green
                : CoverageStatus.Yellow;
        }



        private static bool IsCovered(
            CoverageStatus status)
        {
            return status != CoverageStatus.Red;
        }



        private CoverageStatus Worst(
            System.Collections.Generic.IEnumerable<CoverageStatus> statuses)
        {
            var result = CoverageStatus.Green;

            foreach (var status in statuses)
            {
                if (Rank(status) > Rank(result))
                    result = status;
            }

            return result;
        }



        private static int Rank(CoverageStatus status)
        {
            return status switch
            {
                CoverageStatus.Green => 0,
                CoverageStatus.Yellow => 1,
                CoverageStatus.Red => 2,
                _ => 2
            };
        }
    }
}