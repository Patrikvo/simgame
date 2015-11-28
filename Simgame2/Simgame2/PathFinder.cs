using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simgame2
{
    class PathFinder
    {
        public delegate double GetWeigth(int x, int y);

        public PathFinder(GetWeigth getWeigth, int MaxX, int MaxY, int StartX, int StartY, int GoalX, int GoalY)
        {
            this.getWeigth = getWeigth;
            this.startX = StartX;
            this.startY = StartY;
            this.goalX = GoalX;
            this.goalY = GoalY;
            this.MaxX = MaxX;
            this.MaxY = MaxY;

            SetUp();
            FindPath();
            if (Path != null)
            {
                CompressPath();
            }
            
        }


        public Pair<int>[] Path;

        private GetWeigth getWeigth;
        private int startX;
        private int startY;
        private int goalX;
        private int goalY;
        private int MaxX;
        private int MaxY;

        private void SetUp()
        {
            //Visited = new bool[MaxX * MaxY];
            //Visited[adress(startX, startY)] = true;

            VisitedWithDistance = new double[MaxX * MaxY];
            for (int i = 0; i < VisitedWithDistance.Length; i++) { VisitedWithDistance[i] = double.MaxValue; }

            VisitedWithDistance[adress(startX, startY)] = 0;

            Node n = new Node();
            n.Prev = new Pair<int>[1];
            n.Prev[0] = null;
            n.Coor = new Pair<int>(startX, startY);
            n.RouteCost = 0;
            n.DistanceToGoal = DistanceToGoal(startX, startY, goalX, goalY);
            nodesToExpand = new PriorityQueue<Node>();
            nodesToExpand.Add(n);
        }

        private void FindPath()
        {
            bool GoalReached = false;
            while (nodesToExpand.Count > 0 && !GoalReached)
            {
                Node next = nodesToExpand.Next();
                if (next.Coor.First == goalX && next.Coor.Second == goalY)
                {
                    // found goal
                    Path = new Pair<int>[next.Prev.Length + 1];
                    for (int i = 0; i < next.Prev.Length; i++)
                    {
                        Path[i] = next.Prev[i];
                    }
                    Path[next.Prev.Length] = next.Coor;
                    GoalReached = true;
                    
                    break;
                }

                // expand nodes around current node

                Pair<int>[] expandableNodes = new Pair<int>[8];
                expandableNodes[0] = new Pair<int>(next.Coor.First - 1, next.Coor.Second - 1);
                expandableNodes[1] = new Pair<int>(next.Coor.First    , next.Coor.Second - 1);
                expandableNodes[2] = new Pair<int>(next.Coor.First + 1, next.Coor.Second - 1);
                expandableNodes[3] = new Pair<int>(next.Coor.First - 1, next.Coor.Second    );
                expandableNodes[4] = new Pair<int>(next.Coor.First + 1, next.Coor.Second    );
                expandableNodes[5] = new Pair<int>(next.Coor.First - 1, next.Coor.Second + 1);
                expandableNodes[6] = new Pair<int>(next.Coor.First    , next.Coor.Second + 1);
                expandableNodes[7] = new Pair<int>(next.Coor.First + 1, next.Coor.Second + 1);

                for (int i = 0; i < 8; i++)
                {
                    
                    if (expandableNodes[i].First >= 0 && expandableNodes[i].Second >= 0 && expandableNodes[i].First < MaxX && expandableNodes[i].Second < MaxY)
                    {
                        double TravelWeight = getWeigth(expandableNodes[i].First, expandableNodes[i].Second);
                        if (TravelWeight >= double.MaxValue - 1) 
                        { 
                            continue; 
                        }
                        double cost = next.RouteCost + TravelWeight;


                       // Console.WriteLine("node " + expandableNodes[i].First.ToString() + ", " + expandableNodes[i].Second.ToString() +
                       //     " has weight " + getWeigth(expandableNodes[i].First, expandableNodes[i].Second) + " and cost " + cost);

                        if (cost < 0) 
                        { 
                            cost = double.MaxValue; 
                        }
                        if (VisitedWithDistance[adress(expandableNodes[i].First, expandableNodes[i].Second)] > cost)
                        //if (!IsVisited(expandableNodes[i].First, expandableNodes[i].Second))
                        {
                            //Visit(expandableNodes[i].First, expandableNodes[i].Second);
                            VisitedWithDistance[adress(expandableNodes[i].First, expandableNodes[i].Second)] = cost;

                            Node newNode = new Node();
                            newNode.Coor = expandableNodes[i];
                            newNode.DistanceToGoal = DistanceToGoal(newNode.Coor.First, newNode.Coor.Second, goalX, goalY);

                            // keep track of the path so far.
                            newNode.Prev = new Pair<int>[next.Prev.Length + 1];
                            for (int j = 0; j < next.Prev.Length; j++)
                            {
                                newNode.Prev[j] = next.Prev[j];
                            }
                            newNode.Prev[next.Prev.Length] = next.Coor;

                            newNode.RouteCost = next.RouteCost + getWeigth(newNode.Coor.First, newNode.Coor.Second);
                            // check for overflow
                            if (newNode.RouteCost < 0) 
                            { 
                                newNode.RouteCost = double.MaxValue; 
                            }
 

                            nodesToExpand.Add(newNode);
                        }
                    }
                }
            }


            if (Path == null)
            {
                int a = 0;
            }
        }


        private void CompressPath()
        {
            List<Pair<int>> CompressedPath = new List<Pair<int>>(); ;

            Pair<int> runStart = Path[1];
            direction currentDirection = direction.down;
            direction currentNode;
            for (int i = 2; i < Path.Length; i++)
            {
                currentNode = getDirection(Path[i - 1], Path[i]);
                if (currentNode == currentDirection)
                {
                    // still going in the same direction
                    continue;
                }
                else
                {
                    // changing direction.
                    CompressedPath.Add(runStart);
                    runStart = Path[i];
                    currentDirection = currentNode;
                }
            }
            CompressedPath.Add(runStart);
            CompressedPath.Add(Path[Path.Length - 1]);
            this.Path = CompressedPath.ToArray();
        }

        private direction getDirection(Pair<int> first, Pair<int> second)
        {
            if (first.First == second.First)
            {
                if (first.Second > second.Second)
                {
                    return direction.down;
                }
                else
                {
                    return direction.up;
                }
            }
            else
            {
                if (first.Second == second.Second)
                {
                    if (first.First < second.First)
                    {
                        return direction.right;
                    }
                    else
                    {
                        return direction.left;
                    }
                }
                else
                {
                    if (first.First < second.First)
                    {
                        if (first.Second < second.Second)
                        {
                            return direction.right_up;
                        }
                        else
                        {
                            return direction.right_down;
                        }
                    }
                    else
                    {
                        if (first.Second < second.Second)
                        {
                            return direction.left_up;
                        }
                        else
                        {
                            return direction.down_left;
                        }
                    }
                }
            }
        }


        public enum direction { up, right_up, right, right_down, down, down_left, left, left_up };


        PriorityQueue<Node> nodesToExpand;

        public class Pair<T>
        {
            public Pair()
            {

            }

            public Pair(T first, T second)
            {
                this.First = first;
                this.Second = second;
            }
            public T First;
            public T Second;

            public override string ToString()
            {
                return "(" + First.ToString() + "; " + Second.ToString() + ")";
            }

        }

        private class Node : IComparable<Node>
        {
            public Pair<int>[] Prev;
            public Pair<int> Coor;
            public double RouteCost;
            public double DistanceToGoal;

            public double heuristic
            {
                get { return (RouteCost + DistanceToGoal); }
            }


            public int CompareTo(Node other)
            {
                // If other is not a valid object reference, this instance is greater.
                if (other == null) return 1;

                // The temperature comparison depends on the comparison of 
                // the underlying Double values. 
                return this.heuristic.CompareTo(other.heuristic);
            }

           
        }


      //  bool[] Visited;
        double[] VisitedWithDistance;
        private int adress(int x, int y) { return x + y * MaxX; }
        private double DistanceToGoal(int x, int y, int gx, int gy)
        {
            return (Math.Abs(gx - x) + Math.Abs(gy - y));
            //return (gx - x) * (gx - x) + (gy - y) * (gy - y);
        }

      //  private bool IsVisited(int x, int y) {  return Visited[adress(x,y)];}
       // private void Visit(int x, int y) { Visited[adress(x, y)] = true; }

        public class PriorityQueue<T> where T : IComparable<T>
        {
            /// <summary>Clear all the elements from the priority queue</summary>
            public void Clear()
            {
                mA.Clear();
            }

            /// <summary>Add an element to the priority queue - O(log(n)) time operation.</summary>
            /// <param name="item">The item to be added to the queue</param>
            public void Add(T item)
            {
                // We add the item to the end of the list (at the bottom of the
                // tree). Then, the heap-property could be violated between this element
                // and it's parent. If this is the case, we swap this element with the 
                // parent (a safe operation to do since the element is known to be less
                // than it's parent). Now the element move one level up the tree. We repeat
                // this test with the element and it's new parent. The element, if lesser
                // than everybody else in the tree will eventually bubble all the way up
                // to the root of the tree (or the head of the list). It is easy to see 
                // this will take log(N) time, since we are working with a balanced binary
                // tree.
                int n = mA.Count; mA.Add(item);
                while (n != 0)
                {
                    int p = n / 2;    // This is the 'parent' of this item
                    if (mA[n].CompareTo(mA[p]) >= 0) break;  // Item >= parent
                    T tmp = mA[n]; mA[n] = mA[p]; mA[p] = tmp; // Swap item and parent
                    n = p;            // And continue
                }
            }

            /// <summary>Returns the number of elements in the queue.</summary>
            public int Count
            {
                get { return mA.Count; }
            }

            /// <summary>Returns true if the queue is empty.</summary>
            /// Trying to call Peek() or Next() on an empty queue will throw an exception.
            /// Check using Empty first before calling these methods.
            public bool Empty
            {
                get { return mA.Count == 0; }
            }

            /// <summary>Allows you to look at the first element waiting in the queue, without removing it.</summary>
            /// This element will be the one that will be returned if you subsequently call Next().
            public T Peek()
            {
                return mA[0];
            }

            /// <summary>Removes and returns the first element from the queue (least element)</summary>
            /// <returns>The first element in the queue, in ascending order.</returns>
            public T Next()
            {
                // The element to return is of course the first element in the array, 
                // or the root of the tree. However, this will leave a 'hole' there. We
                // fill up this hole with the last element from the array. This will 
                // break the heap property. So we bubble the element downwards by swapping
                // it with it's lower child until it reaches it's correct level. The lower
                // child (one of the orignal elements with index 1 or 2) will now be at the
                // head of the queue (root of the tree).
                T val = mA[0];
                int nMax = mA.Count - 1;
                mA[0] = mA[nMax]; mA.RemoveAt(nMax);  // Move the last element to the top

                int p = 0;
                while (true)
                {
                    // c is the child we want to swap with. If there
                    // is no child at all, then the heap is balanced
                    int c = p * 2; if (c >= nMax) break;

                    // If the second child is smaller than the first, that's the one
                    // we want to swap with this parent.
                    if (c + 1 < nMax && mA[c + 1].CompareTo(mA[c]) < 0) c++;
                    // If the parent is already smaller than this smaller child, then
                    // we are done
                    if (mA[p].CompareTo(mA[c]) <= 0) break;

                    // Othewise, swap parent and child, and follow down the parent
                    T tmp = mA[p]; mA[p] = mA[c]; mA[c] = tmp;
                    p = c;
                }
                return val;
            }

            /// <summary>The List we use for implementation.</summary>
            List<T> mA = new List<T>();
        }
    }
}
