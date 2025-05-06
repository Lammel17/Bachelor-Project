


public enum AnimationInterruptableType
{ 
    Always_Interruptable = 0,   //like walking                                          | walking
    Easily_Interruptable,       //interruptable by almost any input                     | no walking, but any (attack, item use, evade)
    Hardly_Interruptable,       //interruptable by few input                            | no (attack, item use), but by evade
    Not_Interruptable,          //interruptable by no Input                             | no evade, but get knocked over
    Never_Interruptable         //never interruptable by any input or outside causes    | no knocked over or anything else
} 

//Not can be interrupted by outside means, like getting knocked away, never can never be interrupted

