// list of fixed holidays + all dynamic holidays for years of 1399 and 1400 based on https://time.ir/
// you can pass this exact array or modified version of this arrays as holidays to kamaDatepicker options

const HOLIDAYS = [
	{ month: 1, day: 1 },
	{ month: 1, day: 2 },
	{ month: 1, day: 3 },
	{ month: 1, day: 4 },
	{ month: 1, day: 12 },
	{ month: 1, day: 13 },
	{ month: 3, day: 14 },
	{ month: 3, day: 15 },
	{ month: 11, day: 22 },
	{ month: 12, day: 29 },
	{ year: 1399, month: 1, day: 21 },
	{ year: 1399, month: 2, day: 26 },
	{ year: 1399, month: 3, day: 4 },
	{ year: 1399, month: 3, day: 5 },
	{ year: 1399, month: 3, day: 28 },
	{ year: 1399, month: 5, day: 10 },
	{ year: 1399, month: 5, day: 18 },
	{ year: 1399, month: 6, day: 8 },
	{ year: 1399, month: 6, day: 9 },
	{ year: 1399, month: 7, day: 17 },
	{ year: 1399, month: 7, day: 25 },
	{ year: 1399, month: 7, day: 26 },
	{ year: 1399, month: 8, day: 4 },
	{ year: 1399, month: 8, day: 13 },
	{ year: 1399, month: 10, day: 28 },
	{ year: 1399, month: 12, day: 7 },
	{ year: 1399, month: 12, day: 21 },
	{ year: 1399, month: 12, day: 30 },
	{ year: 1400, month: 1, day: 9 },
	{ year: 1400, month: 2, day: 14 },
	{ year: 1400, month: 2, day: 24 },
	{ year: 1400, month: 2, day: 25 },
	{ year: 1400, month: 3, day: 17 },
	{ year: 1400, month: 4, day: 30 },
	{ year: 1400, month: 5, day: 7 },
	{ year: 1400, month: 5, day: 27 },
	{ year: 1400, month: 5, day: 28 },
	{ year: 1400, month: 7, day: 6 },
	{ year: 1400, month: 7, day: 14 },
	{ year: 1400, month: 7, day: 15 },
	{ year: 1400, month: 7, day: 23 },
	{ year: 1400, month: 8, day: 2 },
	{ year: 1400, month: 10, day: 17 },
	{ year: 1400, month: 11, day: 26 },
	{ year: 1400, month: 12, day: 10 },
	{ year: 1400, month: 12, day: 28 },


	{ year: 1402, month: 11, day: 5 },
	{ year: 1402, month: 11, day: 19 },
	{ year: 1402, month: 12, day: 6 },

	{ year: 1403, month: 1, day: 22 },
	{ year: 1403, month: 1, day: 23 },
	{ year: 1403, month: 2, day: 15 },
	{ year: 1403, month: 3, day: 28 },
	{ year: 1403, month: 4, day: 5 },
	{ year: 1403, month: 4, day: 25 },
	{ year: 1403, month: 4, day: 26 },
	{ year: 1403, month: 6, day: 4 },
	{ year: 1403, month: 6, day: 12 },
	{ year: 1403, month: 6, day: 14 },
	{ year: 1403, month: 6, day: 22 },
	{ year: 1403, month: 6, day: 31 },
	{ year: 1403, month: 9, day: 15 },
	{ year: 1403, month: 10, day: 25 },
	{ year: 1403, month: 11, day: 9 },
	{ year: 1403, month: 11, day: 26 },
	{ year: 1403, month: 12, day: 30 },
];


var customOptions = {
	//nextButtonIcon: "timeir_prev.png"
	//, previousButtonIcon: "timeir_next.png"
	forceFarsiDigits: true
	, markToday: true
	, markHolidays: true
	, highlightSelectedDay: true
	, sync: true
	, gotoToday: true
	
	, futureYearsCount: 3
	, swapNextPrev: false
	, holidays: HOLIDAYS // from kamadatepicker.holidays.js
}


var birthDayOptions = {
	//nextButtonIcon: "timeir_prev.png"
	//, previousButtonIcon: "timeir_next.png"
	forceFarsiDigits: true
	, markToday: true
	, markHolidays: true
	, highlightSelectedDay: true
	, sync: true
	, gotoToday: true
	, pastYearsCount: 100
	, futureYearsCount: 1
	, swapNextPrev: false
	, holidays: HOLIDAYS // from kamadatepicker.holidays.js
}