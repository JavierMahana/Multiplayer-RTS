using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Unity.Entities;

public class GroupStateUI : MonoBehaviour
{
    [Required]
    public Canvas parentCanvas;
    [Required]
    public Button defaultStateButton;
    [Required]
    public Button agresiveStateButton;
    [Required]
    public Button pasiveStateButton;




    private EntityManager entityManager;
    private bool Active => parentCanvas.gameObject.activeInHierarchy;

    void Start()
    {
        Debug.Assert(World.Active != null);
        Debug.Assert(parentCanvas != null);

        entityManager = World.Active.EntityManager;


        defaultStateButton.onClick.AddListener(DefaultStateButtonPressed);
        agresiveStateButton.onClick.AddListener(AgresiveStateButtonPressed);
        pasiveStateButton.onClick.AddListener(PasiveStateButtonPressed);

        parentCanvas.gameObject.SetActive(false);        
    }

    
    void Update()
    {
        var currentSelected = SelectionSystem.CurrentSelection;

        if (currentSelected != null)
        {
            Debug.Assert(entityManager.HasComponent<GroupBehaviour>(currentSelected.entity), "the current selected entity doesn't have a group behaviour component");
            var selectedBehaviour = entityManager.GetComponentData<GroupBehaviour>(currentSelected.entity).Value;
            if (Active)
            {
                SetButtonsInteraction(selectedBehaviour);
            }
            else
            {
                parentCanvas.gameObject.SetActive(true);
                SetButtonsInteraction(selectedBehaviour);

                Debug.Assert(Active, "the group state UI canvas parent is not activated, so we can't activate this UI");
            }
        }
        else
        {
            if (Active)
            {
                parentCanvas.gameObject.SetActive(false);
            }
        }
    }



    private void SetButtonsInteraction(Behaviour selectedBehaviour)
    {
        switch (selectedBehaviour)
        {
            case Behaviour.DEFAULT:
                defaultStateButton.interactable  = false;
                agresiveStateButton.interactable = true;
                pasiveStateButton.interactable   = true;
                break;
            case Behaviour.PASSIVE:
                defaultStateButton.interactable  = true;
                agresiveStateButton.interactable = true;
                pasiveStateButton.interactable   = false;
                break;
            case Behaviour.AGRESIVE:
                defaultStateButton.interactable  = true;
                agresiveStateButton.interactable = false;
                pasiveStateButton.interactable   = true;
                break;
            default:
                throw new System.NotImplementedException();                
        }
    }
    private void DefaultStateButtonPressed()
    {
        var currentSelected = SelectionSystem.CurrentSelection;

        if (currentSelected != null && World.Active != null)
        {
            AddLocalChangeBehaviourCommand(currentSelected.entity, Behaviour.DEFAULT);
        }
        else
        {
            Debug.LogError($"The button is pressed when it is not desired. Entity selected:{currentSelected != null}. World Active:{World.Active != null}");
        }
                   
    }
    private void AgresiveStateButtonPressed()
    {
        var currentSelected = SelectionSystem.CurrentSelection;

        if (currentSelected != null && World.Active != null)
        {
            AddLocalChangeBehaviourCommand(currentSelected.entity, Behaviour.AGRESIVE);
        }
        else
        {
            Debug.LogError($"The button is pressed when it is not desired. Entity selected:{currentSelected != null}. World Active:{World.Active != null}");
        }
    }
    private void PasiveStateButtonPressed()
    {
        var currentSelected = SelectionSystem.CurrentSelection;

        if (currentSelected != null && World.Active != null)
        {
            AddLocalChangeBehaviourCommand(currentSelected.entity, Behaviour.PASSIVE);
        }
        else
        {
            Debug.LogError($"The button is pressed when it is not desired. Entity selected:{currentSelected != null}. World Active:{World.Active != null}");
        }
    }
    private void AddLocalChangeBehaviourCommand(Entity entity, Behaviour newBehaviour)    
    {
        Debug.Assert(entityManager.HasComponent<GroupBehaviour>(entity), "the current selected entity doesn't have a group behaviour component. can't recieve a change behaviour command");

        CommandStorageSystem.TryAddLocalCommand(new ChangeBehaviourCommand()
        {
            Target = entity,
            NewBehaviour = new GroupBehaviour() { Value = newBehaviour }
        }, World.Active);
    }
}
