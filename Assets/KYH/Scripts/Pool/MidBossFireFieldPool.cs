using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class MidBossFireFieldPool : MonoBehaviour
{
    [SerializeField] private MidBossFireFieldPillar fieldPrefab;
    [SerializeField] private Transform storageRoot;
    [SerializeField, Min(1)] private int initialCount = 3;

    [Header("수동 테스트")]
    [SerializeField] private MidBossFireFieldData testData;

    private readonly Queue<MidBossFireFieldPillar> availableFields = new();
    private readonly HashSet<MidBossFireFieldPillar> rentedFields = new();

    public event Action<MidBossFireFieldPillar> FieldReturned;

    private void Awake()
    {
        if (fieldPrefab == null || storageRoot == null)
        {
            Debug.LogError("화염 장판 프리팹과 보관위치를 연결해 주세여.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < initialCount; i++)
            availableFields.Enqueue(CreateField());
    }

    private MidBossFireFieldPillar CreateField()
    {
        MidBossFireFieldPillar field = Instantiate(fieldPrefab, storageRoot);
        field.gameObject.SetActive(false);
        field.Finished += Return;

        return field;
    }

    public MidBossFireFieldPillar Rent(Vector3 position)
    {
        if (!isActiveAndEnabled || !storageRoot.gameObject.activeInHierarchy)
            return null;

            MidBossFireFieldPillar field = availableFields.Count > 0 ? availableFields.Dequeue() : CreateField();

            rentedFields.Add(field);

            field.transform.position = position;
            field.gameObject.SetActive(true);

            return field;
    }

    public void Return(MidBossFireFieldPillar field)
    {
        if (field == null || !rentedFields.Remove(field))
            return;

        field.Stop();
        field.gameObject.SetActive(false);
        availableFields.Enqueue(field);

        FieldReturned?.Invoke(field);
    }

    public void ReturnAll()
    {
        MidBossFireFieldPillar[] fields = new MidBossFireFieldPillar[rentedFields.Count];

        rentedFields.CopyTo(fields);

        foreach (MidBossFireFieldPillar field in fields)
            Return(field);
    }

    [ContextMenu("테스트/화염장판 하나 실행")]
    private void TestRentField()
    {
        if (!Application.isPlaying || testData == null)
            return;

        MidBossFireFieldPillar field = Rent(transform.position);

        if (field == null)
            return;

        if (!field.Begin(testData, transform.position))
        {
            Debug.LogWarning("장판의 경고 참조와 Animator활성 상태를 확인하세요.", this);
            Return(field);
        }
    }

    [ContextMenu("테스트/화염장판 전체 회수")]
    private void TestReturnAll()
    {
        if (Application.isPlaying)
            ReturnAll();
    }
}
