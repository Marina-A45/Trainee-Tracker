/* By Tim Glaser */
function createBurndownChart(canvasId, chartData) {

    console.log("ChartData:", chartData);

    const canvas = document.getElementById(canvasId);

    if (!canvas) {
        console.error("Canvas not found:", canvasId);
        return;
    }

    const ctx = canvas.getContext("2d");

    const gradient = ctx.createLinearGradient(0, 0, canvas.width, 0);
    gradient.addColorStop(0, "#FF0844");
    gradient.addColorStop(1, "#FFB199");

    new Chart(ctx, {
        type: "line",
        data: {
            labels: chartData.labels ?? chartData.Labels,
            datasets: [
                {
                    label: "Ideal Progress",
                    data: chartData.idealProgress ?? chartData.IdealProgress,
                    borderColor: "##141210",
                    backgroundColor: "#141210",
                    order: 2

                },
                {
                    label: "Actual Progress",
                    data: chartData.actualProgress ?? chartData.ActualProgress,
                    borderColor: gradient,
                    backgroundColor: gradient,
                    spanGaps: true,
                    order: 1
                }
            ]
        }
    });
}