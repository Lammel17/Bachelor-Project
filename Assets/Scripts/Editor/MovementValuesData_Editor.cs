using System;
using System.Diagnostics.Eventing.Reader;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MovementValuesData))]
public class MovementValuesData_Editor : PropertyDrawer
{
    [SerializeField] public static bool toggleValueVisibility = false;

    private float curveFieldWidth = 2f;
    private float spaceBetweenValueAndSettings = 10f;
    private float spaceBetweenValueAndInfluence = 10f;
    private float xPacing = 25f;
    private float xExtender = 110f;


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float x = position.x;
        float width = position.width;
        float y = position.y;
        float lineH = EditorGUIUtility.singleLineHeight;
        float vSpace = EditorGUIUtility.standardVerticalSpacing;


        // Grab properties
        var isInUse = property.FindPropertyRelative("isInUse");
        var valueType = property.FindPropertyRelative("valueType");
        var value = property.FindPropertyRelative("value");
        var startEnd = property.FindPropertyRelative("startEnd");
        var curve = property.FindPropertyRelative("curve");

        var influenceType = property.FindPropertyRelative("influenceType");
        var influence = property.FindPropertyRelative("influence");
        var influenceStartEnd = property.FindPropertyRelative("influenceStartEnd");
        var influenceCurve = property.FindPropertyRelative("influenceCurve");

        bool isActive = isInUse.boolValue;

        GUIContent l = new GUIContent();
       if (isActive) l.text = "-    >  " + label.text.ToUpper();
       else l.text = "-        " + label.text.ToUpper();

        // Foldout
        Rect foldRect = new Rect(x - 30, y + 2, width, lineH);
        property.isExpanded = EditorGUI.Foldout(foldRect, true, l , false);

        y += 0 * lineH + vSpace;

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;


        //_________ Start

        var valueSectionStartY = y;
        // IGNORE field
        Rect active = new Rect(x, y, width, EditorGUI.GetPropertyHeight(isInUse));
        EditorGUI.PropertyField(active, isInUse);
        //y += active.height + vSpace;

        if (!isActive && !toggleValueVisibility)
        {
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();

            var usingSectionStartY = vSpace;
            var usingSectionEndY = y + vSpace + active.height + vSpace;

            // draw influence section background
            Rect usingBg = new Rect(
                x,
                usingSectionStartY,
                width,
                usingSectionEndY - usingSectionStartY
            );
            EditorGUI.DrawRect(usingBg, new Color(0.8f, 0.1f, 0.3f, 0.05f)); // redish
            return;
        }
        else if (!isActive && toggleValueVisibility)
        {
            // If ignore, make fields read-only
            bool prevEnabled = GUI.enabled;
            GUI.enabled = false;
        }




        // VALUE SECTION
        // valueType
        Rect rValueType = new Rect(x + xPacing, y, width - xPacing, EditorGUI.GetPropertyHeight(valueType));
        EditorGUI.PropertyField(rValueType, valueType);
        y += rValueType.height + vSpace;

        // value
        Rect rValue = new Rect(x, y, width, EditorGUI.GetPropertyHeight(value));
        EditorGUI.PropertyField(rValue, value);
        y += rValue.height + vSpace + spaceBetweenValueAndSettings;

        // conditional startEnd / curveValue
        switch ((ValueType)valueType.enumValueIndex)
        {
            case ValueType.StartEndValue:
                {
                    Rect rStartEnd = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(startEnd));
                    int oldIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0; // or 0 if you want maximum width
                    EditorGUI.PropertyField(rStartEnd, startEnd, true);
                    EditorGUI.indentLevel = oldIndent;
                    y += rStartEnd.height + vSpace;
                }
                break;

            case ValueType.CurvedValue:
                {
                    Rect rStartEnd = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(startEnd));
                    int oldIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0; // or 0 if you want maximum width
                    EditorGUI.PropertyField(rStartEnd, startEnd, true);
                    EditorGUI.indentLevel = oldIndent;
                    y += rStartEnd.height + vSpace;

                    Rect rCurve = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(curve) * curveFieldWidth);
                    EditorGUI.PropertyField(rCurve, curve);
                    y += rCurve.height + vSpace;
                }
                break;
        }

        var valueSectionEndY =  y + spaceBetweenValueAndInfluence ;

        // Draw value section background
        Rect valueBg = new Rect(
            x,
            valueSectionStartY,
            width,
            valueSectionEndY - valueSectionStartY
        );
        EditorGUI.DrawRect(valueBg, new Color(0.2f, 0.4f, 0.9f, 0.07f)); // light bluish



        // INFLUENCE SECTION
        y += spaceBetweenValueAndInfluence;
        var influenceSectionStartY = y;

        Rect rInfluenceType = new Rect(x, y, width, EditorGUI.GetPropertyHeight(influenceType));
        EditorGUI.PropertyField(rInfluenceType, influenceType);
        y += rInfluenceType.height + vSpace;

        if (influenceType.enumValueIndex != 0)
        {

            Rect rInfluence = new Rect(x, y, width, EditorGUI.GetPropertyHeight(influence));
            EditorGUI.PropertyField(rInfluence, influence);
            y += rInfluence.height + vSpace;

            switch ((InfluenceValueType)influenceType.enumValueIndex )
            {
                case InfluenceValueType.StartEndInfluence:
                    {
                        y += +spaceBetweenValueAndSettings;
                        Rect rInfSE = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(influenceStartEnd));
                        int oldIndent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0; // or 0 if you want maximum width
                        EditorGUI.PropertyField(rInfSE, influenceStartEnd, true);
                        EditorGUI.indentLevel = oldIndent;
                        y += rInfSE.height + vSpace;

                    }
                    break;
                case InfluenceValueType.CurvedInfluence:
                    {
                        y += +spaceBetweenValueAndSettings;

                        Rect rInfSE = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(influenceStartEnd));
                        int oldIndent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0; // or 0 if you want maximum width
                        EditorGUI.PropertyField(rInfSE, influenceStartEnd, true);
                        EditorGUI.indentLevel = oldIndent;
                        y += rInfSE.height + vSpace;

                        Rect rInfCurve = new Rect(x - xExtender, y, width + xExtender, EditorGUI.GetPropertyHeight(influenceCurve) * curveFieldWidth);
                        EditorGUI.PropertyField(rInfCurve, influenceCurve);
                        y += rInfCurve.height + vSpace;
                    }
                    break;
            }
        }
        var influenceSectionEndY = y;

        // draw influence section background
        Rect influenceBg = new Rect(
            x,
            influenceSectionStartY,
            width,
            influenceSectionEndY - influenceSectionStartY
        );
        if (influenceType.enumValueIndex != 0 || influence.floatValue != 1) EditorGUI.DrawRect(influenceBg, new Color(0.1f, 0.8f, 0.3f, 0.05f)); // greenish
        else EditorGUI.DrawRect(influenceBg, new Color(0.1f, 0.8f, 0.3f, 0.02f)); // greenish

        //// restore GUI.enabled
        //GUI.enabled = prevEnabled;

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }



    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float vSpace = EditorGUIUtility.standardVerticalSpacing;

        // If collapsed, only show the foldout line
        if (!property.isExpanded)
            return 0;

        float height = 0 * lineH + vSpace;

        var isInUse = property.FindPropertyRelative("isInUse");
        var valueType = property.FindPropertyRelative("valueType");
        var value = property.FindPropertyRelative("value");
        var startEnd = property.FindPropertyRelative("startEnd");
        var curve = property.FindPropertyRelative("curve");
        var customInfluence = property.FindPropertyRelative("customInfluenceOverInput");
        var influenceType = property.FindPropertyRelative("influenceType");
        var influence = property.FindPropertyRelative("influence");
        var influenceStartEnd = property.FindPropertyRelative("influenceStartEnd");
        var influenceCurve = property.FindPropertyRelative("influenceCurve");
        
        bool isActive = isInUse.boolValue;

        // ignore
        height += EditorGUI.GetPropertyHeight(valueType) + vSpace;
        if (!isActive && !toggleValueVisibility)
            return height;

        // valueType + value
        //height += EditorGUI.GetPropertyHeight(valueType) + vSpace;
        height += EditorGUI.GetPropertyHeight(value) + vSpace + spaceBetweenValueAndSettings;

        switch ((ValueType)valueType.enumValueIndex)
        {
            case ValueType.StartEndValue:
                height += EditorGUI.GetPropertyHeight(startEnd) + vSpace;
                break;
            case ValueType.CurvedValue:
                height += EditorGUI.GetPropertyHeight(startEnd) + vSpace;
                height += EditorGUI.GetPropertyHeight(curve) * curveFieldWidth + vSpace;
                break;
        }

        // customInfluence
        height += EditorGUI.GetPropertyHeight(influenceType) + vSpace + spaceBetweenValueAndSettings;

        if (influenceType.enumValueIndex != 0)
        {
            //height += EditorGUI.GetPropertyHeight(influenceType) + vSpace;
            height += EditorGUI.GetPropertyHeight(influence) + vSpace + spaceBetweenValueAndInfluence;

            switch ((InfluenceValueType)influenceType.enumValueIndex)
            {
                case InfluenceValueType.StartEndInfluence:
                    height += EditorGUI.GetPropertyHeight(influenceStartEnd) + vSpace;
                    break;
                case InfluenceValueType.CurvedInfluence:
                    height += EditorGUI.GetPropertyHeight(influenceStartEnd) + vSpace;
                    height += EditorGUI.GetPropertyHeight(influenceCurve) * curveFieldWidth + vSpace;
                    break;
            }
        }

        return height;
    }

}
