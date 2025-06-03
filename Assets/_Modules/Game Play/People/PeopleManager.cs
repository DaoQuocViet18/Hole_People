using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;
using static EventDefine;

public class PeopleManager : Singleton<PeopleManager>
{
    public void GroupPeopleFindHole (List<GameObject> groupPeople, Node target)
    {
        // Lấy list các cặp (person, đường đi)
        var paths = new List<(GameObject singleGroup, List<Node> path)>();

        foreach (GameObject singleGroup in groupPeople)
        {
            var path = singleGroup.GetComponent<GroupPeopleMovement>().ListGroupWay(target);
            if (path != null && path.Count > 0)
            {
                paths.Add((singleGroup, path));
            }
        }

        // Lấy 8 người có đường đi ngắn nhất (dựa vào độ dài đường đi)
        var shortestPaths = paths
            .OrderBy(p => p.path.Count)
            .Take(8)
            .ToList();

        foreach (var (singleGroup, path) in shortestPaths)
        {
            singleGroup.GetComponent<GroupPeopleMovement>().PeopleMovement(path);
        }
    }
}
