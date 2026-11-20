using System;
using System.Collections.Generic;
using UnityEngine;

public class MidBossFireWavePool : MonoBehaviour
{
    [SerializeField] private MidBossFireWavePillar pillarPrefab;
    [SerializeField] private Transform storageRoot;
    [SerializeField, Min(1)] private int initialCount = 10;

    private readonly Queue<MidBossFireWavePillar> availablePillars = new();
    private readonly HashSet<MidBossFireWavePillar> rentedPillars = new();

    public event Action<MidBossFireWavePillar> EffectStarted;
    public event Action<MidBossFireWavePillar> PillarReturned;
    public event Action<MidBossFireWavePillar, IDamageable> DamageTargetDetected;

    private void Awake()
    {
        if (pillarPrefab == null || storageRoot == null)
        {
            Debug.LogError("불꽃 파동 기둥 프리팹과 보관 위치를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < initialCount; i++)
            availablePillars.Enqueue(CreatePillar());
    }

    private MidBossFireWavePillar CreatePillar()
    {
        MidBossFireWavePillar pillar = Instantiate(pillarPrefab, storageRoot);
        pillar.gameObject.SetActive(false);

        pillar.EffectStarted += HandleEffectStarted;
        pillar.Finished += Return;
        pillar.DamageTargetDetected += HandleDamageTargetDetected;

        return pillar;
    }

    public MidBossFireWavePillar Rent(Vector3 position)
    {
        if (!isActiveAndEnabled || !storageRoot.gameObject.activeInHierarchy)
            return null;

        MidBossFireWavePillar pillar = availablePillars.Count > 0
            ? availablePillars.Dequeue()
            : CreatePillar();

        rentedPillars.Add(pillar);

        pillar.transform.position = position;
        pillar.gameObject.SetActive(true);

        return pillar;
    }

    public void Return(MidBossFireWavePillar pillar)
    {
        if (pillar == null || !rentedPillars.Remove(pillar))
            return;

        pillar.Stop();
        pillar.gameObject.SetActive(false);
        availablePillars.Enqueue(pillar);

        PillarReturned?.Invoke(pillar);
    }

    public void ReturnAll()
    {
        MidBossFireWavePillar[] pillars = new MidBossFireWavePillar[rentedPillars.Count];

        rentedPillars.CopyTo(pillars);

        foreach (MidBossFireWavePillar pillar in pillars)
            Return(pillar);
    }

    private void HandleEffectStarted(MidBossFireWavePillar pillar)
    {
        if (rentedPillars.Contains(pillar))
            EffectStarted?.Invoke(pillar);
    }

    private void HandleDamageTargetDetected(MidBossFireWavePillar pillar, IDamageable target)
    {
        if (rentedPillars.Contains(pillar))
            DamageTargetDetected?.Invoke(pillar, target);
    }

    [ContextMenu("테스트/불꽃파동 기둥 하나 실행")]
    private void TestRentPillar()
    {
        if (!Application.isPlaying)
            return;

        MidBossFireWavePillar pillar = Rent(transform.position);

        if (pillar == null)
            return;

        pillar.BeginWarning(0.5f);
    }

    [ContextMenu("테스트/불꽃파동 전체 회수")]
    private void TestReturnAll()
    {
        if (!Application.isPlaying)
            return;

        ReturnAll();
    }
}
