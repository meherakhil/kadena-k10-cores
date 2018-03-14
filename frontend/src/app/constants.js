/* postfix */
export const FETCH = '_FETCH';
export const SUCCESS = '_SUCCESS';
export const FAILURE = '_FAILURE';

export const SHOW = '_SHOW';
export const HIDE = '_HIDE';

export const START = '_START';
export const FINISH = '_FINISH';

export const INIT_UI = '_INIT_UI';

export const TOGGLE = '_TOGGLE';

export const VALIDATION_ERROR = '_VALIDATION_ERROR';

/* main part */
export const ADD_NEW_ADDRESS = 'ADD_NEW_ADDRESS';
export const LOG_IN = 'LOG_IN';
export const DIALOG = 'DIALOG';
export const LANGUAGES = 'LANGUAGES';
export const DIALOG_ALERT = 'DIALOG_ALERT';
export const APP_LOADING = 'APP_LOADING';
export const SEARCH_PAGE = 'SEARCH_PAGE';
export const ORDER_DETAIL = 'ORDER_DETAIL';
export const HEADER_SHADOW = 'HEADER_SHADOW';
export const SEARCH_RESULTS = 'SEARCH_RESULTS';
export const CHECKOUT_ASK_PDF = 'CHECKOUT_ASK_PDF';
export const CART_PREVIEW = 'CART_PREVIEW';
export const GET_SUBMISSIONID = 'GET_SUBMISSIONID';
export const CART_PREVIEW_CHANGE_ITEMS = 'CART_PREVIEW_CHANGE_ITEMS';
export const SETTINGS_ADDRESSES = 'SETTINGS_ADDRESSES';
export const RECENT_ORDERS = 'RECENT_ORDERS';
export const RECENT_ORDERS_CHANGE_PAGE = 'RECENT_ORDERS_CHANGE_PAGE';
export const MODIFY_MAILING_LIST = 'MODIFY_MAILING_LIST';
export const CHANGE_SEARCH_QUERY = 'CHANGE_SEARCH_QUERY';
export const CHANGE_PAGE_PAGINATOR = 'CHANGE_PAGE_PAGINATOR';

/* checkout */
export const CHECKOUT = 'CHECKOUT';
export const CHECKOUT_INIT_CHECKED_DATA = 'CHECKOUT_INIT_CHECKED_DATA';
export const CHECKOUT_CHANGE_PAYMENT = 'CHECKOUT_CHANGE_PAYMENT';
export const CHECKOUT_CHANGE_QUANTITY = 'CHECKOUT_CHANGE_QUANTITY';
// export const CHECKOUT_REMOVE_PRODUCT = 'CHECKOUT_REMOVE_PRODUCT'; from reducer as well
export const CHECKOUT_CHANGE_ADDRESS = 'CHECKOUT_CHANGE_ADDRESS';
export const CHECKOUT_GET_TOTALS = 'CHECKOUT_GET_TOTALS';
export const CHECKOUT_PROCEED = 'CHECKOUT_PROCEED';
export const CHECKOUT_CHANGE_DELIVERY = 'CHECKOUT_CHANGE_DELIVERY';
export const SAVE_NEW_ADDRESS = 'SAVE_NEW_ADDRESS';
/* refactor */
export const CHECKOUT_INIT_ITEMS = 'CHECKOUT_INIT_ITEMS';
export const CHECKOUT_REMOVE_PRODUCT = 'CHECKOUT_REMOVE_PRODUCT';
export const CHECKOUT_CHANGE_PRODUCT_QUANTITY = 'CHECKOUT_CHANGE_PRODUCT_QUANTITY';

export const ADD_SHIPPING_ADDRESS = 'ADD_SHIPPING_ADDRESS';
export const MANAGE_PRODUCTS = 'MANAGE_PRODUCTS';
export const MODIFY_SHIPPING_ADDRESS = 'MODIFY_SHIPPING_ADDRESS';
export const CHANGE_PAGINATION_LIMIT = 'CHANGE_PAGE_PAGINATION_LIMIT';
export const MODIFY_MAILING_LIST_REPROCESS = 'MODIFY_MAILING_LIST_REPROCESS';
export const MODIFY_MAILING_LIST_USE_CORRECT = 'MODIFY_MAILING_LIST_USE_CORRECT';
export const MODIFY_MAILING_LIST_SHOW_VALIDATION_ERRORS = 'MODIFY_MAILING_LIST_SHOW_VALIDATION_ERRORS';
export const PRODUCTS_LOAD = 'PRODUCTS_LOAD';
export const PRODUCTS_FAVORITE_LOAD = 'PRODUCTS_FAVORITE_LOAD';
export const PRODUCT_MARK_AS_FAVOURITE = 'PRODUCT_MARK_AS_FAVOURITE';
export const PRODUCT_UNMARK_AS_FAVOURITE = 'PRODUCT_UNMARK_AS_FAVOURITE';
export const MAILING_RESPONSE_SUCCESS = 'MAILING/MAILING_RESPONSE_SUCCESS';
export const CARD_VALIDATION_ERROR = 'CARD_VALIDATION_ERROR';
export const SUBMIT_CARD = 'SUBMIT_CARD';
export const SET_ADDRESS_DEFAULT = 'SET_ADDRESS_DEFAULT';
export const UNSET_ADDRESS_DEFAULT = 'UNSET_ADDRESS_DEFAULT';
export const TAC_CHECK = 'TaC_CHECK';
export const TAC_ACCEPT = 'TaC_ACCEPT';
export const CREDENTINALS_CHANGE = 'CREDENTINALS_CHANGE';
export const FILTERED_RECENT_ORDERS_GET_ORDERS = 'FILTERED_RECENT_ORDERS_GET_ORDERS';
export const FILTERED_RECENT_ORDERS_GET_CAMPAIGNS = 'FILTERED_RECENT_ORDERS_GET_CAMPAIGNS';

/* config */
export const isDevelopment = process.env.NODE_ENV === 'development';
export const DOMAIN = 'com';
