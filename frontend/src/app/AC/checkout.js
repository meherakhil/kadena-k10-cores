import axios from 'axios';
import { SHOPPING_CART_UI_FETCH, SHOPPING_CART_UI_SUCCESS, SHOPPING_CART_UI_FAILURE, CHANGE_SHOPPING_DATA,
  INIT_CHECKED_SHOPPING_DATA, RECALCULATE_SHOPPING_PRICE_FETCH, RECALCULATE_SHOPPING_PRICE_SUCCESS,
  RECALCULATE_SHOPPING_PRICE_FAILURE, SEND_SHOPPING_DATA_FETCH, SEND_SHOPPING_DATA_FAILURE,
  SEND_SHOPPING_DATA_SUCCESS, REMOVE_PRODUCT_FETCH, REMOVE_PRODUCT_SUCCESS,
  REMOVE_PRODUCT_FAILURE, CHANGE_PRODUCT_QUANTITY_FAILURE, CHANGE_PRODUCT_QUANTITY_FETCH,
  CHANGE_PRODUCT_QUANTITY_SUCCESS, APP_LOADING_START, APP_LOADING_FINISH, CHECKOUT_ASK_PDF_FETCH,
  CHECKOUT_ASK_PDF_SUCCESS, CHECKOUT_ASK_PDF_FAILURE } from 'app.consts';
import { CHECKOUT } from 'app.globals';
import ui from 'app.ws/checkoutUI';

export const getUI = () => {
  return (dispatch) => {
    dispatch({ type: SHOPPING_CART_UI_FETCH });

    setTimeout(() => {
      dispatch({
        type: SHOPPING_CART_UI_SUCCESS,
        payload: {
          ui: ui.payload,
          isWaitingPDF: ui.payload.submit.isDisabled
        }
      });
    }, 3000);

    // axios.get(CHECKOUT.initUIURL)
    //   .then((response) => {
    //     const { payload, success, errorMessage } = response.data;
    //
    //     if (!success) {
    //       dispatch({ type: SHOPPING_CART_UI_FAILURE });
    //       alert(errorMessage); // eslint-disable-line no-alert
    //       return;
    //     }
    //
    //     dispatch({
    //       type: SHOPPING_CART_UI_SUCCESS,
    //       payload: {
    //         ui: payload,
    //         isWaitingPDF: payload.submit.isDisabled
    //       }
    //     });
    //   })
    //   .catch((error) => {
    //     alert(error); // eslint-disable-line no-alert
    //     dispatch({ type: SHOPPING_CART_UI_FAILURE });
    //   });
  };
};

export const initCheckedShoppingData = (data) => {
  return (dispatch) => {
    dispatch({
      type: INIT_CHECKED_SHOPPING_DATA,
      payload: {
        ...data
      }
    });
  };
};

export const removeProduct = (id) => {
  return (dispatch) => {
    dispatch({
      type: APP_LOADING_START
    });

    const url = CHECKOUT.removeProductURL;

    axios.post(url, { id })
      .then((response) => {
        dispatch({ type: APP_LOADING_FINISH });

        const { payload, success, errorMessage } = response.data;

        if (!success) {
          dispatch({ type: REMOVE_PRODUCT_FAILURE });
          alert(errorMessage); // eslint-disable-line no-alert
          return;
        }

        dispatch({
          type: REMOVE_PRODUCT_SUCCESS,
          payload: {
            ui: payload,
            isWaitingPDF: payload.submit.isDisabled
          }
        });
      })
      .catch((error) => {
        alert(error); // eslint-disable-line no-alert
        dispatch({ type: REMOVE_PRODUCT_FAILURE });
        dispatch({ type: APP_LOADING_FINISH });
      });

    // setTimeout(() => {
    //   dispatch({ type: APP_LOADING_FINISH });
    // }, 2000);

  };
};

export const changeProductQuantity = (id, quantity) => {
  return (dispatch) => {
    dispatch({
      type: APP_LOADING_START
    });

    const url = CHECKOUT.changeQuantityURL;

    axios.post(url, { id, quantity })
      .then((response) => {
        dispatch({ type: APP_LOADING_FINISH });

        const { payload, success, errorMessage } = response.data;

        if (!success) {
          dispatch({ type: CHANGE_PRODUCT_QUANTITY_FAILURE });
          alert(errorMessage); // eslint-disable-line no-alert
          return;
        }

        dispatch({
          type: CHANGE_PRODUCT_QUANTITY_SUCCESS,
          payload: {
            ui: payload,
            isWaitingPDF: payload.submit.isDisabled
          }
        });
      })
      .catch((error) => {
        alert(error); // eslint-disable-line no-alert
        dispatch({ type: CHANGE_PRODUCT_QUANTITY_FAILURE });
        dispatch({ type: APP_LOADING_FINISH });
      });

    // setTimeout(() => {
    //   dispatch({ type: APP_LOADING_FINISH });
    // }, 2000);
  };
};

export const changeShoppingData = (field, id, invoice) => {
  return (dispatch) => {
    dispatch({
      type: CHANGE_SHOPPING_DATA,
      payload: {
        field, id, invoice
      }
    });

    if (field === 'paymentMethod') return;

    dispatch({ type: RECALCULATE_SHOPPING_PRICE_FETCH });

    dispatch({ type: APP_LOADING_START });

    let url = '';
    if (field === 'deliveryMethod') {
      url = CHECKOUT.changeDeliveryMethodURL;
    } else if (field === 'deliveryAddress') {
      url = CHECKOUT.changeAddressURL;
    }

    // setTimeout(() => {
    //   dispatch({
    //     type: RECALCULATE_SHOPPING_PRICE_SUCCESS,
    //     payload: {
    //       ui: ui2.payload
    //     }
    //   });
    //
    //   dispatch({ type: APP_LOADING_FINISH });
    // }, 1000);

    axios.post(url, { id })
      .then((response) => {
        dispatch({ type: APP_LOADING_FINISH });

        const { payload, success, errorMessage } = response.data;

        if (!success) {
          dispatch({ type: RECALCULATE_SHOPPING_PRICE_FAILURE });
          alert(errorMessage); // eslint-disable-line no-alert
          return;
        }

        dispatch({
          type: RECALCULATE_SHOPPING_PRICE_SUCCESS,
          payload: {
            ui: payload,
            isWaitingPDF: payload.submit.isDisabled
          }
        });
      })
      .catch((error) => {
        alert(error); // eslint-disable-line no-alert
        dispatch({ type: RECALCULATE_SHOPPING_PRICE_FAILURE });
        dispatch({ type: APP_LOADING_FINISH });
      });
  };
};

export const sendData = (data) => {
  return (dispatch) => {
    dispatch({ type: SEND_SHOPPING_DATA_FETCH });
    dispatch({ type: APP_LOADING_START });

    axios.post(CHECKOUT.submitURL, { ...data })
      .then((response) => {
        dispatch({ type: APP_LOADING_FINISH });

        const { payload, success, errorMessage } = response.data;

        if (!success) {
          dispatch({ type: SEND_SHOPPING_DATA_FAILURE });
          alert(errorMessage); // eslint-disable-line no-alert
          return;
        }

        dispatch({
          type: SEND_SHOPPING_DATA_SUCCESS,
          payload: {
            status: success,
            redirectURL: payload.redirectURL
          }
        });
      })
      .catch((error) => {
        alert(error); // eslint-disable-line no-alert
        dispatch({ type: APP_LOADING_FINISH });
        dispatch({ type: SEND_SHOPPING_DATA_FAILURE });
      });

    // setTimeout(() => {
    //   dispatch({ type: APP_LOADING_FINISH });
    // }, 2000);
  };
};

export const checkPDFAvailability = () => {
  return (dispatch) => {
    dispatch({ type: CHECKOUT_ASK_PDF_FETCH });

    setTimeout(() => {
      axios.get(CHECKOUT.submittableURL)
        .then((response) => {
          const { payload, success, errorMessage } = response.data;

          if (!success) {
            dispatch({ type: CHECKOUT_ASK_PDF_FAILURE });
            if (success !== undefined) {
              alert(errorMessage); // eslint-disable-line no-alert
            } else {
              alert('ERROR: Missing PDF'); // eslint-disable-line no-alert
            }
            return;
          }

          dispatch({
            type: CHECKOUT_ASK_PDF_SUCCESS,
            payload: {
              isWaitingPDF: !payload // true -> is bad for service and good for me
            }
          });
        })
        .catch((error) => {
          alert(error); // eslint-disable-line no-alert
          dispatch({ type: CHECKOUT_ASK_PDF_FAILURE });
        });

      // dispatch({
      //   type: CHECKOUT_ASK_PDF_SUCCESS,
      //   payload: {
      //     isWaitingPDF: false
      //   }
      // });
    }, 1500);
  };
};
