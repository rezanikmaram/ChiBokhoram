function RenderChartDashboard(allusers, Usages, notUsages, deActives) {
    var roundUsage = Math.round((Usages / allusers) * 100);
    var roundnot = Math.round((notUsages / allusers) * 100);
    var rounddeactive = Math.round((deActives / allusers) * 100);

    let perlistPercent = [];
    let perlistCount = [];

    perlistPercent.push(100, roundUsage, roundnot, rounddeactive);
    perlistCount.push(allusers, Usages, notUsages, deActives);


    let UserCountOptions = {

        series: perlistPercent
        ,
        chart: {
            height: 450,
            type: 'radialBar',
        },
        plotOptions: {
            radialBar: {
                offsetY: 0,
                startAngle: 0,
                endAngle: 270,
                hollow: {
                    margin: 5,
                    size: '30%',
                    background: 'transparent',
                    image: undefined,
                },
                dataLabels: {
                    name: {
                        show: false,
                    },
                    value: {
                        show: false,
                    }
                }
            }
        },
        colors: ['#2e9c03', '#ebb500', '#F44336', '#E9E9E'],
        labels: [`کل پرسنل (${perlistCount[0]} نفر)`,
        `استفاده می کنند (${perlistCount[1]} نفر)`,
        `استفاده نمی کنند (${perlistCount[2]} نفر)`,
        `پرسنل غیرفعال (${perlistCount[3]} نفر)`],
        legend: {
            show: true,
            fontFamily: 'iransans',
            floating: true,
            fontSize: '14px',
            position: 'left',
            offsetX: -25,
            offsetY: 10,
            labels: {
                useSeriesColors: true,
            },
            markers: {
                size: 2
            },
            formatter: function (seriesName, opts) {
                return seriesName + ":  " + opts.w.globals.series[opts.seriesIndex]
            },
            itemMargin: {
                vertical: 3
            }
        },
        responsive: [{
            breakpoint: 480,
            options: {
                legend: {
                    show: false
                }
            }
        }]
    };


    var chartUserCount = new ApexCharts(document.querySelector("#Circlechart"), UserCountOptions);
    chartUserCount.render();
}







