public abstract class MacroEditor
{
    public abstract void OnGUI();
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void OnSelectionChange() { }
    public virtual bool IsValidForSelection => true;
}
