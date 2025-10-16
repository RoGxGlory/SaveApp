using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour {

 public Animator initiallyOpen;

 private int m_OpenParameterId;
 private Animator m_Open;
 private Animator m_PreviousOpen;
 private GameObject m_PreviouslySelected;

 const string k_OpenTransitionName = "Open";
 const string k_ClosedStateName = "Closed";

 public void OnEnable()
 {
  m_OpenParameterId = Animator.StringToHash (k_OpenTransitionName);

  if (initiallyOpen == null)
   return;

  OpenPanel(initiallyOpen);
 }

 // Wrapper so Unity Inspector (supports 0 or 1 parameter) can call OpenPanel
 public void OpenPanel(Animator anim)
 {
  OpenPanel(anim, false);
 }
 
 public void OpenPanelInstant(Animator anim)
 {
  OpenPanel(anim, true);
 }
 
 // Added optional 'instant' parameter
 public void OpenPanel (Animator anim, bool instant = false)
 {
  if (m_Open == anim)
   return;

  var newPreviouslySelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
  anim.gameObject.SetActive(true);
  anim.transform.SetAsLastSibling();

  m_PreviouslySelected = newPreviouslySelected;

  // store the currently open animator as the previous before closing
  m_PreviousOpen = m_Open;

  if (instant)
  {
   if (m_Open != null)
   {
    var prev = m_Open;
    prev.SetBool(m_OpenParameterId, false);
    SetSelected(m_PreviouslySelected);
    m_Open = null;
    prev.gameObject.SetActive(false);
   }
  }
  else
  {
   CloseCurrent();
  }

  m_Open = anim;
  m_Open.SetBool(m_OpenParameterId, true);

  GameObject go = FindFirstEnabledSelectable(anim.gameObject);
  SetSelected(go);
 }
 
 public void OpenSettingsPanel (Animator anim)
 {
  if (m_Open == anim)
   return;

  var newPreviouslySelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
  anim.gameObject.SetActive(true);
  anim.transform.SetAsLastSibling();

  m_PreviouslySelected = newPreviouslySelected;
  
  CloseCurrent();
  
  m_Open = anim;
  m_Open.SetBool(m_OpenParameterId, true);

  GameObject go = FindFirstEnabledSelectable(anim.gameObject);
  SetSelected(go);
 }

 static GameObject FindFirstEnabledSelectable (GameObject gameObject)
 {
  GameObject go = null;
  var selectables = gameObject.GetComponentsInChildren<Selectable> (true);
  foreach (var selectable in selectables) {
   if (selectable.IsActive () && selectable.IsInteractable ()) {
    go = selectable.gameObject;
    break;
   }
  }
  return go;
 }

 // Added optional 'instant' parameter
 public void CloseCurrent(bool instant = false)
 {
  if (m_Open == null)
   return;

  // capture the animator to avoid races with m_Open being changed by OpenPanel
  var previous = m_Open;
  previous.SetBool(m_OpenParameterId, false);

  SetSelected(m_PreviouslySelected);

  // clear m_Open immediately so other logic knows there's no open panel
  m_Open = null;

  if (instant)
  {
   previous.gameObject.SetActive(false);
  }
  else
  {
   StartCoroutine(DisablePanelDelayed(previous));
  }
 }

 IEnumerator DisablePanelDelayed(Animator anim)
 {
  bool closedStateReached = false;
  bool wantToClose = true;
  while (!closedStateReached && wantToClose)
  {
   if (!anim.IsInTransition(0))
    closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

   wantToClose = !anim.GetBool(m_OpenParameterId);

   yield return new WaitForEndOfFrame();
  }

  if (wantToClose)
   anim.gameObject.SetActive(false);
 }

 private void SetSelected(GameObject go)
 {
  if (EventSystem.current != null)
   EventSystem.current.SetSelectedGameObject(go);
 }
 public void ReturnToPrevious(bool instant = true)
 {
  if (m_PreviousOpen == null)
   return;

  // close current and open the previous that was stored
  if (instant)
  {
   CloseCurrent(instant);
   OpenPanel(m_PreviousOpen, instant);
  }
  else
  {
   CloseCurrent();
   OpenPanel(m_PreviousOpen);
  }
  m_PreviousOpen = null;
 }
 
}

