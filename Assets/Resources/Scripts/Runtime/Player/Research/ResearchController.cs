using Items;
using UnityEngine;

public class ResearchController : MonoBehaviour, IBookPageController
{
    #region Private fields
    [SerializeField]
    private UIResearchPage _researchPage;
    [SerializeField]
    private ResearchesSO _researchData;
    private int _indexOfActiveResearch;

    #endregion

    #region Public fields

    #endregion

    #region Properties
    public bool IsActive => UIManager.Instance.ResearchUI.IsActive;

    public ResearchesSO ResearchData
    {
        get
        {
            return _researchData;
        }

        set
        {
            _researchData = value;
        }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        PrepareData();
        PrepareUI();
    }

    public void PrepareData()
    {
        _researchData.Initialize();
        _researchData.OnResearchChangedState += HandleUpdateUIResearchState;
    }

    public void PrepareUI()
    {
        int researchesCount = _researchData.GetResearchesCount();
        _researchPage.InitializePage(researchesCount);
        for (int i = 0; i < _researchData.GetResearchesCount(); i++)
        {
            _researchPage.UpdateResearch(
                i,
                _researchData.GetName(i),
                _researchData.GetIconImage(i),
                _researchData.GetPosition(i),
                _researchData.GetState(i));
            foreach (ResearchSO parent in _researchData.GetListOfPerents(i))
            {
                _researchPage.CreateResearchLine(i, _researchData.GetPosition(i), _researchData.GetPosition(parent));
            }
        }
        _researchPage.OnTryFinishResearch += HandleTryFinishResearch;
        _researchPage.OnResearchDescriptionRequested += HandleResearchDescriptionRequested;
        _researchPage.OnRewardDescriptionRequested += HandleRewardDescriptionRequested;
        _researchPage.OnCostDescriptionRequested += HandleCostDescriptionRequested;
    }

    public void ResetData()
    {
        UIManager.Instance.ResearchUI.ReverseActivity();
        _researchPage.ResetPage();
    }

    private void HandleTryFinishResearch(int index)
    {
        if (_researchData.ExamineResearch(index))
        {
            _researchPage.FinishResearch(index);
        }
    }

    private void HandleResearchDescriptionRequested(int index)
    {
        _indexOfActiveResearch = index;
        _researchPage.UpdateResearchDescription(_researchData.GetName(index), _researchData.GetDescription(index));
        foreach (var reward in _researchData.GetListOfRewards(index))
        {
            _researchPage.AddRewardToResearchDescription(reward.Result.Item.SmallItemImage);
        }
        foreach (var cost in _researchData.GetListOfCosts(index))
        {
            _researchPage.AddCostToResearchDescription(cost.Item.SmallItemImage, cost.Quantity);
        }
    }

    private void HandleRewardDescriptionRequested(int index)
    {
        ItemSO reward = _researchData.GetListOfRewards(_indexOfActiveResearch)[index].Result.Item;
        _researchPage.UpdateItemDescription(reward.SmallItemImage, reward.ColoredName, reward.Description);
    }

    private void HandleCostDescriptionRequested(int index)
    {
        ItemSO cost = _researchData.GetListOfCosts(_indexOfActiveResearch)[index].Item;
        _researchPage.UpdateItemDescription(cost.SmallItemImage, cost.ColoredName, cost.Description);
    }

    private void HandleUpdateUIResearchState(int index, ResearchState state)
    {
        _researchPage.UpdateResearchState(index, state);
    }
    #endregion
}
