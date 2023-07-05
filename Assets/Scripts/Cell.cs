
public class Cell {
    public bool isWater;
    public int height;

    public Cell(bool isWater) {
        this.isWater = isWater;
    }

    public Cell(bool isWater, int height) {
        this.isWater = isWater;
        this.height = height;
    }
}
