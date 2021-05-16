var table = null;
$(function () {
    table = $('#marketPairsTable').DataTable({
        ajax: {
            url: "Markets/MarketPairs",
            data: function (d) {
                var signalNames = $('#signalsFilter').find(":selected").map(function () {
                    return $.trim($(this).text());
                }).get();

                if (signalNames.length) {
                    d.signalsFilter = signalNames;
                } else {
                    d.signalsFilter = [];
                }
            },
            type: "POST",
            dataSrc: ""
        },
        columns: [
            {
                className: 'control',
                orderable: false,
                data: null,
                defaultContent: ''
            },
            {
                name: "Name",
                data: "name",
                render: function (data, type, row, meta) {
                    var outlineStyle = row.hasTradingPair ? "info" : "secondary";
                    var element = '<div style="width: 120px"><a href="https://www.tradingview.com/chart/?symbol=' + row.tradingViewName + '" target = "_blank" class="btn btn-outline-' + outlineStyle + ' btn-sm">' + data + '</a>';
                    if (row.signalRules.length) {
                        element += '&nbsp;<i class="fas fa-bolt text-info" title="Trailing"></i>';
                    }
                    element += '</div>';
                    return element;
                }
            },
            {
                name: "Rating",
                data: "ratingList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value '
                            + (item.value != null && item.value >= 0 ? 'text-success' : 'text-warning') + '">'
                            + (item.value != null ? item.value.toFixed(3) : "N/A")
                            + '</span></div>';
                    }).join("");
                }
            },
            {
                name: "RatingChange",
                data: "ratingChangeList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value '
                            + (item.value != null && item.value >= 0 ? 'text-success' : 'text-warning') + '">'
                            + (item.value != null ? item.value.toFixed(2) : "N/A")
                            + '</span></div>';
                    }).join("");
                },
                visible: false
            },
            {
                name: "Price",
                data: "price",
                render: function (data, type, row, meta) {
                    return data;
                }
            },
            {
                name: "PriceChange",
                data: "priceChangeList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value '
                            + (item.value != null && item.value >= 0 ? 'text-success' : 'text-warning') + '">'
                            + (item.value != null ? item.value.toFixed(2) : "N/A")
                            + '</span></div>';
                    }).join("");
                }
            },
            {
                name: "Spread",
                data: "spread",
                render: function (data, type, row, meta) {
                    return data;
                }
            },
            {
                name: "Arbitrage",
                data: "arbitrageList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value">'
                            + (item.arbitrage != null ? item.arbitrage : "N/A")
                            + '</span></div>';
                    }).join("");
                }
            },
            {
                name: "Volume",
                data: "volumeList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value">'
                            + (item.volume != null ? item.volume : "N/A")
                            + '</span></div>';
                    }).join("");
                }
            },
            {
                name: "VolumeChange",
                data: "volumeChangeList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value '
                            + (item.value != null && item.value >= 0 ? 'text-success' : 'text-warning') + '">'
                            + (item.value != null ? item.value.toFixed(2) : "N/A")
                            + '</span></div>';
                    }).join("");
                },
                visible: false
            },
            {
                name: "Volatility",
                data: "volatilityList",
                type: "multi-value",
                render: function (data, type, row, meta) {
                    return data.map(function (item) {
                        return '<div class="signal-details"><span class="signal-name">'
                            + item.name + '</span><span class="signal-value">'
                            + (item.value != null ? item.value.toFixed(2) : "N/A")
                            + '</span></div>';
                    }).join("");
                }
            },
            {
                name: "TradingRules",
                data: "config",
                render: function (data, type, row, meta) {
                    return data.rules.join("<br/>");
                }
            },
            {
                name: "SignalRules",
                data: "signalRules",
                render: function (data, type, row, meta) {
                    return data.join("<br/>");
                }
            }
        ],
        order: [[2, "desc"]],
        responsive: {
            details: {
                type: "column"
            }
        },
        pageLength: 25,
        colReorder: true,
        stateSave: true,
        dom: 'Bfrtp',
        buttons: [
            {
                extend: "colvis",
                text: "Columns"
            },
            "copy",
            "csv",
            {
                text: 'Log',
                action: function (e, dt, node, config) {
                    $('#logEntries').collapse('toggle');
                }
            }
        ]
    });

    table.search();

    $.fn.dataTable.ext.type.order['multi-value-pre'] = function (d) {
        return getMultiValueAvg(d);
    };

    $.fn.dataTable.ext.search.push(
        function (settings, searchData, dataIndex) {
            var show = true;
            $(".filter").each(function (idx, element) {
                var filter = $(element);
                var value = parseFloat(filter.val());
                var valueIndex = filter.data("index");
                if (value && valueIndex) {
                    var visibleData = [];
                    for (var idx in searchData) {
                        if (settings.aoColumns.filter(function (col) { return col.idx == idx; })[0].bVisible) {
                            visibleData.push(searchData[idx]);
                        }
                    }

                    var data = visibleData[valueIndex + 1];
                    if (!isNaN(data)) {
                        if (parseFloat(data) < value) {
                            show = false;
                        }
                    } else {
                        var sum = getMultiValueAvg(data);
                        if (sum < value) {
                            show = false;
                        }
                    }
                }
            });
            return show;
        }
    );

    $('#marketPairsTable thead th:not(.control)').each(function (i) {
        var title = $('#marketPairsTable thead th').eq($(this).index()).text();
        if (title != "Name" && title != "Trading Rules" && title != "Trailing Rules" && title != "Signal Rules") {
            $(this).prepend('<input type="text" class="filter" onclick="return filterClicked(event);" placeholder="Min ' + title + '" data-index="' + i + '" />');
        }
    });

    $(table.table().container()).on('keyup', 'thead input', function () {
        table.draw();
    });

    $('#marketPairsTable tbody').on('click', 'td:not(:first-child)', function (ev) {
        if (ev.target.tagName === "A")
            return;
        var tr = $(this).closest('tr');
        var row = table.row(tr);
        if (row.child.isShown()) {
            hideRow(row);
        }
        else {
            showRow(row);
        }
    });

    function getMultiValueAvg(element) {
        var total = 0;
        var sum = 0;
        $(element).find(".signal-value").each(function (idx, element) {
            var value = parseFloat(element.innerText);
            if (!value) value = 0;
            sum += value;
            total++;
        });
        return sum / total;
    }

    setInterval(function () {
        refreshTable();
    }, 5000);

    document.addEventListener("visibilitychange", function () {
        refreshTable();
    }, false);

    $.get("Markets/MarketSignals", function (data) {
        $('<div class="signals-filter"><select id="signalsFilter" multiple="multiple"></div>').insertAfter(".dt-buttons");
        var signalsFilter = $("#signalsFilter");
        for (var i = 0; i < data.length; i++) {
            var signalName = data[i];
            signalsFilter.append('<option selected="selected">' + signalName + '</option>');
        }
        signalsFilter.multiselect({
            buttonText: function () {
                return "Signals";
            },
            optionClass: function () {
                return "signal-filter-option";
            },
            onChange: function () {
                refreshTable();
            }
        });
    });
});

function refreshTable() {
    if (!document.hidden && $(".additional-details").length == 0 && $(".dtr-details").length == 0) {
        table.ajax.reload(null, false);
    }
}

function showRow(row) {
    row.child(format(row.data())).show();
    $(row.node()).addClass('shown');
}

function hideRow(row) {
    row.child.hide();
    $(row.node()).removeClass('shown');
}

function format(data) {
    var details = $($("#rowDetails").html());
    details.find("#pair").val(data.name);
    details.find("#amount").attr("value", data.amount).attr("min", 0);
    details.find("#signalRules").text(data.signalRules.join(", "));
    details.find("#tradingRules").text(data.config.rules.join(", "));
    return details.html();
}

function filterClicked(e) {
    e.preventDefault();
    e.stopPropagation();
    return false;
}

function showSettings(e) {
    var pair = $(e).closest(".row-details").find("#pair").val();
    var tr = $(e).closest('tr').prev();
    var row = table.row(tr);
    var config = row.data().config;
    $("#modalTitle").text(pair + " Settings");
    $("#modalContent").html("<pre>" + JSON.stringify(config, null, 4) + "</pre>");
    $("#modal").modal('show');
}

function buyPair(e) {
    var pair = $(e).closest(".row-details").find("#pair").val();
    var amount = $(e).parent().find("#amount").val();
    if (confirm("Buy " + amount + " " + pair + "?")) {
        $.post("Buy", { pair: pair, amount: amount }, function (data) {
            var tr = $(e).closest('tr').prev();
            var row = table.row(tr);
            hideRow(row);
            refreshTable();
        }).fail(function (data) {
            alert("Error buying " + pair);
        });
    }
}

function buyPairDefault(e) {
    var pair = $(e).closest(".row-details").find("#pair").val();
    var amount = $(e).parent().find("#amount").val();
    if (confirm("Buy " + pair + " with default settings?")) {
        $.post("BuyDefault", { pair: pair }, function (data) {
            var tr = $(e).closest('tr').prev();
            var row = table.row(tr);
            hideRow(row);
            refreshTable();
        }).fail(function (data) {
            alert("Error buying " + pair);
        });
    }
}