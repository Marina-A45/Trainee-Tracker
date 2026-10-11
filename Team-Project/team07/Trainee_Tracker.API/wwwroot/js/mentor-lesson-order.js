// Enables trainee-specific ordering of open lessons on the mentor page.
document.addEventListener("DOMContentLoaded", () => {
  let draggedCard = null;

  const traineeSelect = document.getElementById("trainee-filter");

  // Returns the open lesson column of the currently displayed trainee.
  // Author: Dennis Axt.
  const getOpenColumn = () =>
    document.querySelector('.columns--column[data-status="open"]');

  // Track only regular open lessons. Skipped lessons are not reordered.
  document.addEventListener("dragstart", (event) => {
    const card = event.target.closest(".lesson-card");
    const openColumn = getOpenColumn();

    if (!card || !openColumn
        || card.parentElement !== openColumn
        || card.classList.contains("is-skipped")) {
      draggedCard = null;
      return;
    }

    draggedCard = card;

    // Override the status-drag styling because the open column is a valid
    // target for mentor-specific reordering.
    setTimeout(() => {
      openColumn.classList.remove("is-invalid-target");
      openColumn.classList.add("is-valid-target");
    }, 0);
  });

  // Move the dragged card before the card currently below the pointer.
  document.addEventListener("dragover", (event) => {
    const openColumn = event.target.closest(
      '.columns--column[data-status="open"]');

    if (!draggedCard || !openColumn) {
      return;
    }

    event.preventDefault();

    const otherOpenCards = [...openColumn.children]
      .filter(card =>
        card.classList.contains("lesson-card")
        && !card.classList.contains("is-skipped")
        && card !== draggedCard);

    const nextCard = otherOpenCards.find(card => {
      const bounds = card.getBoundingClientRect();
      return event.clientY < bounds.top + bounds.height / 2;
    });

    // Keep skipped cards after all regular open lessons.
    const firstSkippedCard =
      openColumn.querySelector(".lesson-card.is-skipped");

    openColumn.insertBefore(
      draggedCard,
      nextCard || firstSkippedCard);
  });

  // Save the new order when the mentor drops the card inside OPEN.
  document.addEventListener("drop", async (event) => {
    const openColumn = event.target.closest(
      '.columns--column[data-status="open"]');

    if (!draggedCard || !openColumn || !traineeSelect?.value) {
      return;
    }

    event.preventDefault();

    const orderedCards = [...openColumn.children]
      .filter(card =>
        card.classList.contains("lesson-card")
        && !card.classList.contains("is-skipped"));

    const orderedProgressIds =
      orderedCards.map(card => card.id);

    try {
      const response = await fetch("/api/lesson/update-order", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          traineeId: traineeSelect.value,
          orderedProgressIds: orderedProgressIds,
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.message);
      }

    }
    catch (error) {
    // Show a message only when saving fails.
    console.error(error);
    alert(error.message || "The lesson order could not be saved.");
    }
  });

  document.addEventListener("dragend", () => {
    draggedCard = null;
  });
});