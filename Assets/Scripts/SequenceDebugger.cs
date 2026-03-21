#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Editor-only utility to print all sequence answers to the console.
/// Attach to any GameObject and hit Play to see the output.
/// </summary>
public class SequenceDebugger : MonoBehaviour
{
    private void Start()
    {
        // Triangular offset: n => 1 + (n+1)(n+2)/2
        PrintSequence("Triangular offset", n => 1 + (n + 1) * (n + 2) / 2, 6);

        // Alternating x3 / +1
        PrintSequence("Alternating *3/+1", n =>
        {
            int v = 2;
            for (int i = 0; i < n; i++) v = (i % 2 == 0) ? v * 3 : v + 1;
            return v;
        }, 6);

        // Fibonacci-like
        PrintSequence("Fibonacci-like", n =>
        {
            if (n == 0) return 1;
            if (n == 1) return 2;
            int a = 1, b = 2;
            for (int i = 2; i <= n; i++) { int t = a + b; a = b; b = t; }
            return b;
        }, 6);

        // Perfect squares
        PrintSequence("Perfect squares", n => (n + 1) * (n + 1), 6);
    }

    private static void PrintSequence(string name, System.Func<int, int> f, int count)
    {
        var sb = new System.Text.StringBuilder($"[{name}] ");
        for (int i = 0; i < count; i++)
        {
            sb.Append(f(i));
            if (i < count - 1) sb.Append(" -> ");
        }
        Debug.Log(sb.ToString());
    }
}
#endif
