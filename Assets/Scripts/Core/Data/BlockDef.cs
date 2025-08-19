// ============================================================================
// Scripts/Blocks/BlockDef.cs (ScriptableObject шаблон блока/модуля)
// ============================================================================
using UnityEngine;
using System;
using System.Data.Common;

[CreateAssetMenu(menuName = "Robolike/BlockDef", fileName = "BlockDef")]
public class BlockDef : ScriptableObject

{
    [Header("ID & Display")]
    public string id;
    // уникальный строковый ID, например "core.basic"
    public string displayName;
    public GameObject prefab;         // визуал и коллайдер блока (1x1x1 по сетке)

    [Header("Grid & Physics")]
    public Vector3Int size = Vector3Int.one; // кратно 1 клетке
    public float mass = 1f;                  // вклад в массу робота
    public int blockHP = 50;                 // запас прочности самого блока

    [Header("Role")]
    public BlockCategory category = BlockCategory.Armor;

    [Header("Energy Model")]            // Простая энергетика
    public int energyProvide = 0;        // даёт энергию (Battery)
    public int energyConsume = 0;        // потребляет (оружие/мотор)

    [Header("Placement Rules")]
    public bool mustAttachToStructure = true; // запрещает «висящие» блоки
}