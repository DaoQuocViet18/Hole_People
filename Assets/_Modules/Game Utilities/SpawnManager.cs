using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Box Settings")]
    public GameObject[] boxPrefabs;    // Các prefab box khác nhau
    public int columns = 6;            // Số cột
    public int rows = 8;               // Số hàng (em có thể chỉnh)
    public float boxSize = 1f;         // Kích thước mỗi box (khoảng cách giữa các box)

    [Header("Spawn Area")]
    public Transform spawnStartPoint;  // Điểm bắt đầu spawn (góc dưới cùng bên trái)

    private GameObject[,] gridArray;   // Mảng lưu trữ grid

    void Start()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        gridArray = new GameObject[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                SpawnBox(x, y);
            }
        }
    }

    void SpawnBox(int x, int y)
    {
        // Chọn random prefab
        int randomIndex = Random.Range(0, boxPrefabs.Length);
        GameObject boxPrefab = boxPrefabs[randomIndex];

        // Tính vị trí spawn
        Vector3 spawnPos = spawnStartPoint.position + new Vector3(x * boxSize, y * boxSize, 0);

        // Spawn box
        GameObject spawnedBox = Instantiate(boxPrefab, spawnPos, Quaternion.identity, this.transform);

        // Lưu vào grid
        gridArray[x, y] = spawnedBox;
    }
}
