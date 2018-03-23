using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

public class UIButton : MonoBehaviour
, IColliderEventPressUpHandler
, IColliderEventPressEnterHandler
, IColliderEventPressExitHandler
{
    public enum ButtonType
    {
        NewMesh,
        SaveMesh,
        Undo,
        Redo
    }

    public ButtonType type;

	#region IColliderEventPressUpHandler implementation
	public void OnColliderEventPressUp (ColliderButtonEventData eventData)
	{
		Debug.Log ("OnColliderEventPressUp");
        ButtonLogic();
	}

	public void OnColliderEventPressEnter (ColliderButtonEventData eventData)
	{
		Debug.Log ("OnColliderEventPressEnter");
	}

	public void OnColliderEventPressExit (ColliderButtonEventData eventData)
	{
		Debug.Log ("OnColliderEventPressExit");
	}

	#endregion

	void Start()
	{}

    void ButtonLogic()
    {
        switch (type)
        {
            case ButtonType.NewMesh:
                NewMesh();
                break;
            case ButtonType.SaveMesh:
                SaveMesh();
                break;
            case ButtonType.Undo:
                Undo();
                break;
            case ButtonType.Redo:
                Redo();
                break;
        }
    }

    void NewMesh()
    {
        MeshGenerator.Instance.CreateMesh();
    }

    void SaveMesh()
    {
        MeshIOManager.Instance.ExportMesh();
    }

    void Undo()
    {
        DeformManager.Instance.PerformUndo();
    }

    void Redo()
    {
        DeformManager.Instance.PerformRedo();
    }
	
}
