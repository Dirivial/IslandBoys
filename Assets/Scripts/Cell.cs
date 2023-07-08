
public class Cell
{
  public bool isWater;
  public float height;

  public Cell(bool isWater)
  {
    this.isWater = isWater;
  }

  public Cell(bool isWater, float height)
  {
    this.isWater = isWater;
    this.height = height;
  }
}
