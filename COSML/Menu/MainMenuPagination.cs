using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static COSML.Menu.MenuUtils;

namespace COSML.Menu
{
    public class MainMenuPagination : MonoBehaviour
    {
        public AbstractMainMenu menu;

        private HashSet<GameObject> elements;
        private List<GameObject> pages;
        private MainMenuPageSelector pageSelector;
        private int currentPage;

        public void Init()
        {
            elements = new HashSet<GameObject>();
            pages = new List<GameObject>();
            currentPage = 0;

            pageSelector = CreatePageSelector();
        }

        public void AddElement(GameObject element)
        {
            elements.Add(element);

            GameObject lastPage = pages.Count > 0 ? pages.Last() : null;
            if ((elements.Count - 1) % OPTION_MENU_MAX_PER_PAGE == 0)
            {
                lastPage = CreatePage();
                pageSelector.SetValues(pages.Select((m, i) => i + 1).ToArray());
                pageSelector.gameObject.SetActive(pages.Count > 1);
            }

            element.transform.SetParent(lastPage.transform, false);
            pageSelector.gameObject.SetActive(pages.Count > 1);
        }

        public void InitRoll()
        {
            pageSelector?.InitRoll();
        }

        public void ForceExit()
        {
            pageSelector?.ForceExit();
        }

        public void Loop()
        {
            pageSelector?.Loop();
        }

        private GameObject CreatePage()
        {
            GameObject pageGo = new($"Page_{pages.Count + 1}");
            pageGo.transform.SetParent(transform, false);
            pageGo.SetActive(pages.Count == 0);

            pages.Add(pageGo);

            return pageGo;
        }

        public void ChangePage(int page)
        {
            if (pages == null || pages.Count == 0) return;

            pages[currentPage].SetActive(false);
            currentPage = Mathf.Clamp(page, 0, pages.Count - 1);
            pages[currentPage].SetActive(true);
        }

        public int GetCurrentPage()
        {
            return currentPage;
        }

        public OverableUI[][] GetOverableUI()
        {
            int index = 0;
            OverableUI[][] overableUIs = new OverableUI[pages[currentPage].transform.childCount][];
            foreach (Transform btn in pages[currentPage].transform)
            {
                OverableUI btnOver = btn.GetComponent<MainMenuButton>();
                btnOver ??= btn.GetComponent<MainMenuText>()?.over;
                btnOver ??= btn.GetComponent<MainMenuSelector>()?.over;
                btnOver ??= btn.GetComponent<MainMenuSlider>()?.over;
                btnOver ??= btn.GetComponent<MainMenuInputButton>()?.over;

                overableUIs[index] = new OverableUI[1] { btnOver };
                index++;
            }

            return overableUIs;
        }

        private MainMenuPageSelector CreatePageSelector()
        {
            // Base
            MainMenuSelector select = CreateSelect(new InternalSelectData
            {
                parent = transform,
                menu = menu,
                label = "PAGE",
                values = null,
                value = 0,
                buttonId = Constants.PAGINATION_BUTTON_ID,
                position = 0
            });
            MainMenuPageSelector pageSelector = select.gameObject.AddComponent<MainMenuPageSelector>();
            pageSelector.menu = menu;
            pageSelector.pagination = this;
            pageSelector.over = select.over;
            pageSelector.over.gameObject.SetActive(false);
            pageSelector.overAnimator = select.overAnimator;
            pageSelector.valueText = select.valueText;
            pageSelector.valueText.transform.localPosition = new Vector3(-375, pageSelector.valueText.transform.localPosition.y, 0);
            pageSelector.SetValues(new int[1] { 1 });
            pageSelector.transform.localPosition = new Vector3(1000, transform.Find("Text_Titre").localPosition.y + 18, 0);

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
