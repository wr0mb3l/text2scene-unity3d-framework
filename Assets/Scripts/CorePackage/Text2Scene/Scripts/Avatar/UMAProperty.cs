using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAProperty
{
    public static List<UMAProperty> PROPERTIES_LIST = new List<UMAProperty>();
    // DNA Keys
    // armLength, armWidth, belly,  breastSize, cheekPosition, cheekSize, chinPosition, chinPronounced, chinSize, earsPosition,
    // earsRotation, earsSize, eyeRotation, eyeSpacing, eyeSize, feetSize, forearmLength, forearmWidth, foreheadPosition, foreheadSize,
    // gluteusSize, handsSize, headSize, headWidth, height, jawsPosition, jawsSize, legSeparation, legsSize, lipsSize, lowCheekPosition,
    // lowCheekPronounced, lowerMuscle, lowerWeight, mandibleSize, mouthSize, neckThickness, noseCurve, noseFlatten, noseInclination,
    // nosePosition, nosePronounced, noseSize, noseWidth, skinBlueness, skinGreenness, skinRedness, upperMuscle, upperWeight, waist,
    public string Value { get; private set; }
    public bool IsDNA { get; private set; }
    public bool IsSpecial { get; private set; }

    protected UMAProperty(string value, bool isDNA = true, bool isSpecial = false)
    {
        Value = value;
        IsDNA = isDNA;
        IsSpecial = isSpecial;
        PROPERTIES_LIST.Add(this);
    }

    public static UMAProperty SKIN_COLOR = new UMAProperty("skinColor", false, true);
    public static UMAProperty MODEL = new UMAProperty("model", false, true);

    public static UMAProperty ARM_LENGTH = new UMAProperty("armLength", true);
    public static UMAProperty ARM_WIDTH = new UMAProperty("armWidth", true);
    public static UMAProperty BELLY = new UMAProperty("belly", true);
    public static UMAProperty BREAST_SIZE = new UMAProperty("breastSize", true);
    public static UMAProperty CHEEK_POSITION = new UMAProperty("cheekPosition", true);
    public static UMAProperty CHEEK_SIZE = new UMAProperty("cheekSize", true);
    public static UMAProperty CHIN_POSITION = new UMAProperty("chinPosition", true);
    public static UMAProperty CHIN_PRONOUNCED = new UMAProperty("chinPronounced", true);
    public static UMAProperty CHIN_SIZE = new UMAProperty("chinSize", true);
    public static UMAProperty EARS_POSITION = new UMAProperty("earsPosition", true);
    public static UMAProperty EARS_ROTATION = new UMAProperty("earsRotation", true);
    public static UMAProperty EARS_SIZE = new UMAProperty("earsSize", true);
    public static UMAProperty EYE_ROTATION = new UMAProperty("eyeRotation", true);
    public static UMAProperty EYE_SPACING = new UMAProperty("eyeSpacing", true);
    public static UMAProperty EYE_SIZE = new UMAProperty("eyeSize", true);
    public static UMAProperty FEET_SIZE = new UMAProperty("feetSize", true);
    public static UMAProperty FOREARM_LENGTH = new UMAProperty("forearmLength", true);
    public static UMAProperty FOREARM_POSTION = new UMAProperty("forearmWidth", true);
    public static UMAProperty FOREHEAD_POSTION = new UMAProperty("foreheadPosition", true);
    public static UMAProperty FOREHEAD_SIZE = new UMAProperty("foreheadSize", true);
    public static UMAProperty GLUTEUS_SIZE = new UMAProperty("gluteusSize", true);
    public static UMAProperty HAND_SIZE = new UMAProperty("handsSize", true);
    public static UMAProperty HEAD_SIZE = new UMAProperty("headSize", true);
    public static UMAProperty HEAD_WIDTH = new UMAProperty("headWidth", true);
    public static UMAProperty HEIGHT = new UMAProperty("height", true);
    public static UMAProperty JAWS_POSITION = new UMAProperty("jawsPosition", true);
    public static UMAProperty JAWS_SIZE = new UMAProperty("jawsSize", true);
    public static UMAProperty LEG_SEPERATION = new UMAProperty("legSeparation", true);
    public static UMAProperty LEGS_SIZE = new UMAProperty("legsSize", true);
    public static UMAProperty LIPS_SIZE = new UMAProperty("lipsSize", true);
    public static UMAProperty LOW_CHEEK_POSITION = new UMAProperty("lowCheekPosition", true);
    public static UMAProperty LOW_CHEEK_PRONOUNCED = new UMAProperty("lowCheekPronounced", true);
    public static UMAProperty LOWER_MUSCLE = new UMAProperty("lowerMuscle", true);
    public static UMAProperty LOWER_WEIGHT = new UMAProperty("lowerWeight", true);
    public static UMAProperty MANDIBLE_SIZE = new UMAProperty("mandibleSize", true);
    public static UMAProperty MOUTH_SIZE = new UMAProperty("mouthSize", true);
    public static UMAProperty NECK_THICKNESS = new UMAProperty("neckThickness", true);
    public static UMAProperty NOSE_CURVE = new UMAProperty("noseCurve", true);
    public static UMAProperty NOSE_FLATTEN = new UMAProperty("noseFlatten", true);
    public static UMAProperty NOSE_INCLANATION = new UMAProperty("noseInclination", true);
    public static UMAProperty NOSE_POSITION = new UMAProperty("nosePosition", true);
    public static UMAProperty NOSE_PRONOUNCED = new UMAProperty("nosePronounced", true);
    public static UMAProperty NOSE_SIZE = new UMAProperty("noseSize", true);
    public static UMAProperty NOSE_WIDTH = new UMAProperty("noseWidth", true);
    public static UMAProperty SKIN_BLUENESS = new UMAProperty("skinBlueness", true);
    public static UMAProperty SKIN_GREENNESS = new UMAProperty("skinGreenness", true);
    public static UMAProperty SKIN_REDNESS = new UMAProperty("skinRedness", true);
    public static UMAProperty UPPER_MUSCLE = new UMAProperty("upperMuscle", true);
    public static UMAProperty UPPER_WEIGHT = new UMAProperty("upperWeight", true);
    public static UMAProperty WAIST = new UMAProperty("waist", true);

    public static UMAProperty WARDROBE_HAIR = new UMAProperty("Hair", false);
    public static UMAProperty WARDROBE_CHEST = new UMAProperty("Chest", false);
    public static UMAProperty WARDROBE_LEGS = new UMAProperty("Legs", false);
    public static UMAProperty WARDROBE_UNDERWEAR = new UMAProperty("Underwear", false);
    public static UMAProperty WARDROBE_BEARD = new UMAProperty("Beard", false);
    public static UMAProperty WARDROBE_EYEBROW = new UMAProperty("Eyebrow", false);
    public static UMAProperty WARDROBE_EYES = new UMAProperty("Eyes", false);
    public static UMAProperty WARDROBE_FACE = new UMAProperty("Face", false);
    public static UMAProperty WARDROBE_HANDS = new UMAProperty("Hands", false);

    //public static List<UMAProperty> PROPERTIES_LIST = new List<UMAProperty>
    //{
    //    ARM_LENGTH, ARM_WIDTH, BELLY, BREAST_SIZE, CHEEK_POSITION, CHEEK_SIZE, CHIN_POSITION, CHIN_PRONOUNCED, CHIN_SIZE,
    //    EARS_POSITION, EARS_ROTATION, EARS_SIZE, EYE_ROTATION, EYE_SPACING, EYE_SIZE, FEET_SIZE, FOREARM_LENGTH, FOREARM_POSTION,
    //    FOREHEAD_POSTION, FOREHEAD_SIZE, GLUTEUS_SIZE, HAND_SIZE, HEAD_SIZE, HEAD_WIDTH, HEIGHT, JAWS_POSITION, JAWS_SIZE, LEG_SEPERATION,
    //    LEGS_SIZE, LIPS_SIZE, LOW_CHEEK_POSITION, LOW_CHEEK_PRONOUNCED, LOWER_MUSCLE, LOWER_WEIGHT, MANDIBLE_SIZE, MOUTH_SIZE,
    //    NECK_THICKNESS, NOSE_CURVE, NOSE_FLATTEN, NOSE_INCLANATION, NOSE_POSITION, NOSE_PRONOUNCED, NOSE_SIZE, NOSE_WIDTH, SKIN_BLUENESS,
    //    SKIN_GREENNESS, SKIN_REDNESS, UPPER_MUSCLE, UPPER_WEIGHT, WAIST, WARDROBE_HAIR, WARDROBE_CHEST, WARDROBE_LEGS, WARDROBE_UNDERWEAR,
    //    WARDROBE_BEARD, WARDROBE_EYEBROW, WARDROBE_EYES, WARDROBE_FACE, WARDROBE_HANDS
    //};

    public static UMAProperty Find(string value)
    {
        return PROPERTIES_LIST.Find(p => p.Value.Equals(value));
    }
}
