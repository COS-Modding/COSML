using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static COSML.MainMenu.MenuUtils;

namespace COSML.MainMenu
{
    public class MainMenuPagination : MonoBehaviour
    {
        public AbstractMainMenu menu;

        private List<MonoBehaviour> options;
        private MainMenuPageSelector pageSelector = null;
        private int totalPages = 1;
        private int currentPage;

        public int CurrentPage => currentPage;

        public void Init(List<MonoBehaviour> optionsMono)
        {
            options = optionsMono;
            totalPages = 1;
            currentPage = 0;

            pageSelector = CreatePageSelector();

            CreateOptionsList();
        }

        private void CreateOptionsList()
        {
            foreach (MonoBehaviour option in options)
            {
                GameObject optionGo = new("MenuOption");
                optionGo.transform.SetParent(transform, false);
                option.transform.SetParent(optionGo.transform, false);
            }

            RefreshPageSelector();
        }

        private void RefreshPageSelector()
        {
            int visibleCount = options.Select(o => o.gameObject.activeSelf ? 1 : 0).Sum();
            totalPages = Mathf.Max(Mathf.CeilToInt((float)visibleCount / OPTION_MENU_MAX_PER_PAGE), 1);
            pageSelector.SetValues([.. Enumerable.Range(1, totalPages)]);
            pageSelector.gameObject.SetActive(totalPages > 1);
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
        }

        public void InitRoll()
        {
            pageSelector?.InitRoll();
        }

        public void Loop()
        {
            pageSelector?.Loop();
        }

        public void ForceExit()
        {
            pageSelector?.ForceExit();
        }

        public void ChangePage(int page)
        {
            if (totalPages <= 1) return;
            currentPage = Mathf.Clamp(page, 0, totalPages - 1);
            if (menu is ModsMainMenu modsMainMenu) modsMainMenu.SetIndex(0, 0);
            else if (menu is ModMenu modMenu) modMenu.SetIndex(0, 0);
            Refresh();
        }

        public OverableUI[][] GetOverableUI()
        {
            RefreshPageSelector();

            List<OverableUI[]> overableUIs = [];
            int visibleIndex = 0;
            int activeIndex = 0;
            foreach (MonoBehaviour option in options)
            {
                GameObject menuOption = option.transform.parent.gameObject;
                menuOption.SetActive(IsInPage(activeIndex));
                if (!option.gameObject.activeSelf) continue;
                activeIndex++;
                if (!menuOption.activeSelf) continue;

                OverableUI optionOver = option.GetComponent<Patches.MainMenuButton>();
                optionOver ??= option.GetComponent<MainMenuText>()?.over;
                optionOver ??= option.GetComponent<MainMenuSelector>()?.over;
                optionOver ??= option.GetComponent<MainMenuSlider>()?.over;
                optionOver ??= option.GetComponent<MainMenuInputText>()?.over;

                option.transform.localPosition = GetOptionButtonLocalPosition(visibleIndex);
                overableUIs.Add([optionOver]);
                visibleIndex++;
            }

            return [.. overableUIs];
        }

        private bool IsInPage(int index) => index >= currentPage * OPTION_MENU_MAX_PER_PAGE && index < (currentPage + 1) * OPTION_MENU_MAX_PER_PAGE;

        private MainMenuPageSelector CreatePageSelector()
        {
            if (this.pageSelector != null) return this.pageSelector;

            // Base
            MainMenuSelector select = CreateSelect(new SelectData
            {
                parent = transform,
                menu = menu,
                label = new I18nKey("cosml.menu.mods.page"),
                values = [],
                value = 0,
                buttonId = Constants.PAGINATION_BUTTON_ID,
            });
            select.name = "MenuPagination";
            select.transform.localPosition = GetOptionButtonLocalPosition(0);
            MainMenuPageSelector pageSelector = select.gameObject.AddComponent<MainMenuPageSelector>();
            pageSelector.transform.localPosition = new Vector3(1000, transform.Find("Text_Titre").localPosition.y + 18, 0);
            pageSelector.menu = menu;
            pageSelector.pagination = this;
            pageSelector.over = select.over;
            pageSelector.over.gameObject.SetActive(false);
            pageSelector.overAnimator = select.overAnimator;
            pageSelector.valueText = select.valueText;
            pageSelector.valueText.transform.localPosition = new Vector3(-375, pageSelector.valueText.transform.localPosition.y, 0);
            pageSelector.SetValues([1]);

            // Label
            Text labelText = pageSelector.transform.Find("Text_Libellé").GetComponent<Text>();
            labelText.alignment = TextAnchor.MiddleRight;
            labelText.transform.localPosition = new Vector3(-1220, labelText.transform.localPosition.y, 0);

            // Chevrons
            pageSelector.left = select.left;
            pageSelector.left.transform.Find("ChevronPrev_over").GetComponent<Image>().sprite = MenuResources.MainMenuChevronOverWhite;
            pageSelector.left.Init(pageSelector);
            pageSelector.left.transform.localPosition = new Vector3(-500, pageSelector.left.transform.localPosition.y, 0);
            pageSelector.right = select.right;
            pageSelector.right.transform.Find("ChevronNext_over").GetComponent<Image>().sprite = MenuResources.MainMenuChevronOverWhite;
            pageSelector.right.Init(pageSelector);
            pageSelector.right.transform.localPosition = new Vector3(-250, pageSelector.right.transform.localPosition.y, 0);

            // Pad indications
            pageSelector.padIndics = new GameObject("PadIndics");
            pageSelector.padIndics.transform.SetParent(pageSelector.transform, false);

            pageSelector.prevPagePicto = new GameObject("PrevPagePicto").AddComponent<Image>();
            pageSelector.prevPagePicto.GetComponent<RectTransform>().sizeDelta = new Vector2(256, 128);
            pageSelector.prevPagePicto.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            pageSelector.prevPagePicto.transform.SetParent(pageSelector.padIndics.transform, false);
            pageSelector.prevPagePicto.transform.localPosition = new Vector3(pageSelector.left.transform.localPosition.x + 48, 32, 0);

            pageSelector.nextPagePicto = new GameObject("NextPagePicto").AddComponent<Image>();
            pageSelector.nextPagePicto.GetComponent<RectTransform>().sizeDelta = new Vector2(256, 128);
            pageSelector.nextPagePicto.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            pageSelector.nextPagePicto.transform.SetParent(pageSelector.padIndics.transform, false);
            pageSelector.nextPagePicto.transform.localPosition = new Vector3(pageSelector.right.transform.localPosition.x - 48, 32, 0);

            Destroy(pageSelector.transform.Find("Fond_over").gameObject);
            Destroy(select);

            return pageSelector;
        }
    }
}
