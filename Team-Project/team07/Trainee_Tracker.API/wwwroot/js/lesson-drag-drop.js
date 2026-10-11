// Feedback-only rating and rejection workflow cleanup by Dennis Axt.
document.addEventListener("DOMContentLoaded", () => {
  // Select all elements representing cards and columns
  const cards = document.querySelectorAll(".lesson-card");
  const columns = document.querySelectorAll(".columns--column");

  // Trackers for active element mutations during a drag operation
  let draggedCard = null;
  let sourceStatus = null;

  // Use the global window object generated via Razor
  const LessonStatus = window.LessonStatus || {};

  // Maps HTML DOM lowercase data-status attributes to JS Enum values
  const statusMap = {
    open: LessonStatus.Open,
    started: LessonStatus.Started,
    finished: LessonStatus.Finished,
    rejected: LessonStatus.Rejected,
    accepted: LessonStatus.Accepted,
    rated: LessonStatus.Rated,
    skipped: LessonStatus.Skipped,
  };

  // Read workflow guidelines from window or default fallback
  const allowedTransitions = window.AllowedTransitions || {
    open: ["started", "skipped"],
    started: ["open", "finished"],
    finished: ["started"],
    rejected: ["finished"],
    accepted: [],
    rated: [],
    skipped: ["open"],
  };

  // Safely extract and normalize data-status attribute from an element or its ancestor
  function getElementStatus(element) {
    if (!element) return null;
    const target = element.closest("[data-status]");
    return target ? target.getAttribute("data-status")?.toLowerCase() : null;
  }

  // Safely resolve the string status to the backend enum integer
  function getStatusEnumValue(statusStr) {
    if (!statusStr) return undefined;
    const key = statusStr.toLowerCase();

    if (statusMap[key] !== undefined) {
      return statusMap[key];
    }

    // Case-insensitive fallback directly on window.LessonStatus
    const matchingKey = Object.keys(LessonStatus).find(
      (k) => k.toLowerCase() === key,
    );
    return matchingKey !== undefined ? LessonStatus[matchingKey] : undefined;
  }

  // Validates if moving a card between two specific statuses is allowed
  function isTransitionValid(fromStatus, toStatus) {
    if (!fromStatus || !toStatus) return false;
    if (fromStatus.toLowerCase() === toStatus.toLowerCase()) return false;
    const transitions = allowedTransitions[fromStatus.toLowerCase()];
    return transitions?.includes(toStatus.toLowerCase()) || false;
  }

  // Sorts all lesson cards inside a container element by data-position
  function sortCardsInContainer(container) {
    if (!container) return;

    const cardsArray = Array.from(container.querySelectorAll(".lesson-card"));

    cardsArray.sort((a, b) => {
      const posA = parseFloat(a.getAttribute("data-position")) || 0;
      const posB = parseFloat(b.getAttribute("data-position")) || 0;
      return posA - posB;
    });

    cardsArray.forEach((card) => container.appendChild(card));
  }

  // Initialize layout updates for all columns on page load
  columns.forEach(sortCardsInContainer);

  // Bind drag events to individual cards
  cards.forEach((card) => {
    card.addEventListener("dragstart", (e) => {
      draggedCard = card;
      sourceStatus = getElementStatus(card);

      card.classList.add("is-dragging");
      e.dataTransfer.setData("text/plain", card.id);

      setTimeout(() => {
        columns.forEach((col) => {
          const targetStatus = getElementStatus(col);
          if (isTransitionValid(sourceStatus, targetStatus)) {
            col.classList.add("is-valid-target");
          } else {
            col.classList.add("is-invalid-target");
          }
        });
      }, 0);
    });

    card.addEventListener("dragend", () => {
      card.classList.remove("is-dragging");
      columns.forEach((col) =>
        col.classList.remove(
          "is-valid-target",
          "is-invalid-target",
          "drag-over",
        ),
      );

      draggedCard = null;
      sourceStatus = null;
    });
  });

  // Bind drop events across column containers
  columns.forEach((column) => {
    column.addEventListener("dragover", (e) => {
      const targetStatus = getElementStatus(column);
      if (draggedCard && isTransitionValid(sourceStatus, targetStatus)) {
        e.preventDefault();
        column.classList.add("drag-over");
      }
    });

    column.addEventListener("dragleave", () => {
      column.classList.remove("drag-over");
    });

    column.addEventListener("drop", (e) => {
      e.preventDefault();
      column.classList.remove("drag-over");

      const targetStatus = getElementStatus(column);

      if (draggedCard && isTransitionValid(sourceStatus, targetStatus)) {
        const lessonId = draggedCard.id;
        const newStatusValue = getStatusEnumValue(targetStatus);

        if (newStatusValue === undefined) {
          console.error(
            `Could not resolve status "${targetStatus}" to an enum value.`,
          );
          return;
        }

        // 1. Physically move card to destination column
        column.appendChild(draggedCard);

        // 2. Apply new column attributes, buttons, and state rules
        updateCardForColumn(draggedCard, targetStatus);

        // 3. Sort column cards
        sortCardsInContainer(column);

        // 4. Send API update
        fetch("/api/lesson/update-status", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            "X-SignalR-Connection-Id": window.currentConnectionId || "",
          },
          body: JSON.stringify({
            lessonId: lessonId,
            newStatus: newStatusValue,
          }),
        }).catch((error) => console.error("Sync Error:", error));
      }
    });
  });
});

function updateCardForColumn(card, targetStatus) {
  // 1. Toggle Draggable Attribute & Cursor Classes
  if (targetStatus === "rated") {
    card.setAttribute("draggable", "false");
    card.classList.add("no-hand", "is-skipped");
  } else {
    card.setAttribute("draggable", "true");
    card.classList.remove("no-hand", "is-skipped");
  }

  // 2. Clean up or toggle rejection modifier
  if (targetStatus !== "rejected") {
    card.classList.remove("is-rejected");
    const rejectionReasonEl = card.querySelector(".rejection-reason");
    if (rejectionReasonEl) rejectionReasonEl.remove();
  } else {
    card.classList.add("is-rejected");
  }

  // 3. Manage "Give Feedback" button for Accepted column
  const existingFeedbackBtn = card.querySelector(".feedback-btn");

  if (targetStatus === "accepted") {
    if (!existingFeedbackBtn) {
      const progressId = card.id;
      const feedbackBtn = document.createElement("a");
      feedbackBtn.className = "feedback-btn custom-btn";
      feedbackBtn.style.display = "inline-block";
      feedbackBtn.style.backgroundImage = "var(--gradient-Pink-Peach)";
      feedbackBtn.style.border = "none";
      feedbackBtn.style.borderRadius = "4px";
      feedbackBtn.style.padding = "0.25rem 0.75rem";
      feedbackBtn.style.font = "inherit";
      feedbackBtn.style.fontSize = "0.9rem";
      feedbackBtn.style.color = "var(--color-Gray1)";
      feedbackBtn.style.textDecoration = "none";
      feedbackBtn.style.cursor = "pointer";
      feedbackBtn.style.marginTop = "0.5rem";
      feedbackBtn.href = `/Trainee/Feedback?progressId=${progressId}`;
      feedbackBtn.textContent = "Give Feedback";
      //card.appendChild(feedbackBtn);
    }
  } else if (existingFeedbackBtn) {
    // Remove feedback button if moved out of 'accepted'
    existingFeedbackBtn.remove();
  }
}
