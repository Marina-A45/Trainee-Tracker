//dummy data planner

const weeklyLearningPlan = {

    events: [
        {
            id: 1,
            title: "Start here!",
            start: "2026-06-29",
            end: "2026-06-30",
            url: "https://makandracards.com/curriculum/34973-start-0d",
            position: 1,
            timeEstimate: 0,
            status: "Rated",
            isStartable: false
        },
        {
            id: 2,
            title: "Ruby basics",
            start: "2026-06-30",
            end: "2026-07-01",
            url: "https://makandracards.com/curriculum/34329-ruby-basics-2d",
            position: 2,
            timeEstimate: 1,
            status: "Accepted",
            isStartable: false
        },
        {
            id: 120,
            title: "Git basics",
            start: "2026-06-30",
            end: "2026-07-04",
            url: "https://makandracards.com/curriculum/34335-git-basics-1d",
            position: 3,
            timeEstimate: 4,
            status: "Finished",
            isStartable: false
        },
        {
            id: 110,
            title: "Where to find API documentation",
            start: "2026-07-06",
            end: "2026-07-07",
            url: "https://makandracards.com/curriculum/35167-find-api-documentation-0-5d",
            position: 4,
            timeEstimate: 1,
            status: "Started",
            isStartable: false
        },
        {
            id: 125,
            title: "Gems, bundler, rbenv",
            start: "2026-07-06",
            end: "2026-07-07",
            url: "https://makandracards.com/curriculum/34333-gems-bundler-rbenv-1d",
            position: 5,
            timeEstimate: 1,
            status: "Open",
            isStartable: true
        }
    ],

    blockedDays: [
        "2026-06-22",
       
        "2026-06-24",
        "2026-06-25",
        "2026-06-26",
        "2026-06-27",
        "2026-06-28"
    ]
};