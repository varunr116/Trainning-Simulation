
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Scene2AudioData", menuName = "Training/Scene 2 Audio Data")]
public class Scene2AudioData : ScriptableObject
{
    [Header("Welcome & Instructions")]
    public AudioClip welcomeToWarehouse;
    public AudioClip generalInstructions;
    
    [Header("Pickup Audio (When P is pressed)")]
    public AudioClip pickupTapeGun;
    public AudioClip pickupBarcodeScanner;
    public AudioClip pickupSafetyGloves;
    public AudioClip pickupSafetyGoggles;
    public AudioClip pickupGeneric;
    
    [Header("Drop Audio (When D is pressed)")]
    public AudioClip dropTapeGun;
    public AudioClip dropBarcodeScanner;
    public AudioClip dropSafetyGloves;
    public AudioClip dropSafetyGoggles;
    public AudioClip dropGeneric;
    
    [Header("Progress Audio")]
    public AudioClip firstItemCollected;
    public AudioClip halfwayComplete;
    public AudioClip almostFinished;
    public AudioClip allItemsCollected;
    
    [Header("Timer Audio")]
    public AudioClip twoMinutesRemaining;
    public AudioClip thirtySecondsRemaining;
    public AudioClip timeUp;
}

